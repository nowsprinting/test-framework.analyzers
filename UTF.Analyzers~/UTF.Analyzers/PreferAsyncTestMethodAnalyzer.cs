using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4006: Async Task test methods are recommended over coroutine-style test methods with the UnityTest attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferAsyncTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4006";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Async Task test methods are recommended over coroutine-style test methods with the UnityTest attribute",
        messageFormat:
        "'{0}' is a coroutine-style test method that yields only Unity yield instructions: it cannot use TestCase attributes or catch an exception across a wait. Use an 'async Task' test method instead.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a coroutine-style test method whose every yield return yields null or a Unity yield instruction, so that the method can be written as an async Task test method with no other change. A coroutine-style test method that yields anything else, e.g. an IEnumerator returned by the code under test, is not reported.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4006.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        var testMethods = TestMethodAnalysis.TryCreate(compilation);
        var yieldInstruction = compilation.GetTypeByMetadataName("UnityEngine.YieldInstruction");
        var customYieldInstruction = compilation.GetTypeByMetadataName("UnityEngine.CustomYieldInstruction");
        var coroutine = compilation.GetTypeByMetadataName("UnityEngine.Coroutine");
        if (testMethods is null || yieldInstruction is null || customYieldInstruction is null || coroutine is null)
        {
            return;
        }

        var enumerator = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);
        context.RegisterSymbolAction(c =>
        {
            var method = (IMethodSymbol)c.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator)
                || !testMethods.IsTestMethod(method))
            {
                return;
            }

            var walk = new Walk(compilation, yieldInstruction, customYieldInstruction, coroutine, method.ContainingType,
                c.CancellationToken);
            if (walk.YieldsOnlyUnityInstructions(method, 0))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
            }
        }, SymbolKind.Method);
    }

    // DepthBoundedWalker is not reused: it descends into lambdas and local functions, whereas a yield in a lambda
    // belongs to the lambda and must not count for the method; and an operand it cannot see through must exempt the
    // method here rather than go unreported.
    private sealed class Walk
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _yieldInstruction;
        private readonly INamedTypeSymbol _customYieldInstruction;
        private readonly INamedTypeSymbol _coroutine;
        private readonly INamedTypeSymbol _fixture;
        private readonly CancellationToken _cancellationToken;

        public Walk(Compilation compilation, INamedTypeSymbol yieldInstruction,
            INamedTypeSymbol customYieldInstruction, INamedTypeSymbol coroutine, INamedTypeSymbol fixture,
            CancellationToken cancellationToken)
        {
            _compilation = compilation;
            _yieldInstruction = yieldInstruction;
            _customYieldInstruction = customYieldInstruction;
            _coroutine = coroutine;
            _fixture = fixture;
            _cancellationToken = cancellationToken;
        }

        // A method with no yield statement hands over an enumerator built elsewhere, which the walk cannot inspect.
        // The depth bound also ends a recursive helper, so no in-progress set is needed.
        public bool YieldsOnlyUnityInstructions(IMethodSymbol method, int depth)
        {
            if (depth > DepthBoundedWalker.MaxDepth
                || OperationAnalysis.MethodBody(_compilation, method, _cancellationToken) is not { } body)
            {
                return false;
            }

            var isIterator = false;
            foreach (var yield in Yields(body))
            {
                isIterator = true;
                if (yield.ReturnedValue is { } returned
                    && !IsConvertible(OperationAnalysis.WithoutImplicitConversions(returned), depth))
                {
                    return false;
                }
            }

            return isIterator;
        }

        // Yields in a lambda or local function belong to that function, not to the method.
        private IEnumerable<IReturnOperation> Yields(IOperation operation)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            switch (operation)
            {
                case IAnonymousFunctionOperation:
                case ILocalFunctionOperation:
                    yield break;
                case IReturnOperation { Kind: OperationKind.YieldReturn or OperationKind.YieldBreak } yield:
                    yield return yield;
                    yield break;
            }

            foreach (var child in operation.ChildOperations)
            {
                foreach (var yield in Yields(child))
                {
                    yield return yield;
                }
            }
        }

        private bool IsConvertible(IOperation yielded, int depth)
        {
            if (yielded is ILiteralOperation { ConstantValue: { HasValue: true, Value: null } })
            {
                return true;
            }

            if (yielded.Type is INamedTypeSymbol type && IsUnityInstruction(type))
            {
                return true;
            }

            // A helper on any other type, including a test-only MonoBehaviour, is the code under test.
            var callee = yielded is IInvocationOperation invocation ? invocation.TargetMethod.OriginalDefinition : null;
            return callee is not null
                   && (SymbolEqualityComparer.Default.Equals(callee.ContainingType, _fixture)
                       || ActionAttributeAnalysis.DerivesFrom(_fixture, callee.ContainingType))
                   && YieldsOnlyUnityInstructions(callee, depth + 1);
        }

        // The namespace is checked rather than the assembly: in tests the Unity types are dummies compiled into the
        // test assembly. Coroutine derives from YieldInstruction but stands for a coroutine of the code under test.
        private bool IsUnityInstruction(INamedTypeSymbol type)
        {
            var ns = type.ContainingNamespace.ToDisplayString();
            return !SymbolEqualityComparer.Default.Equals(type, _coroutine)
                   && (string.Equals(ns, "UnityEngine", StringComparison.Ordinal)
                       || ns.StartsWith("UnityEngine.", StringComparison.Ordinal))
                   && (ActionAttributeAnalysis.DerivesFrom(type, _yieldInstruction)
                       || ActionAttributeAnalysis.DerivesFrom(type, _customYieldInstruction));
        }
    }
}
