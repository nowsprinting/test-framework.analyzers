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

        var walk = new Walk(compilation, testMethods, yieldInstruction, customYieldInstruction, coroutine);
        context.RegisterSymbolAction(walk.AnalyzeMethod, SymbolKind.Method);
    }

    // DepthBoundedWalker is not reused: it descends into lambdas and local functions, whereas a yield in a lambda
    // belongs to the lambda and must not count for the method; and an operand it cannot see through must exempt the
    // method here rather than go unreported. One instance serves the whole compilation, so the per-method state
    // (fixture, cancellation token) is passed along instead of stored.
    private sealed class Walk
    {
        private readonly Compilation _compilation;
        private readonly TestMethodAnalysis _testMethods;
        private readonly INamedTypeSymbol _enumerator;
        private readonly INamedTypeSymbol _yieldInstruction;
        private readonly INamedTypeSymbol _customYieldInstruction;
        private readonly INamedTypeSymbol _coroutine;

        public Walk(Compilation compilation, TestMethodAnalysis testMethods, INamedTypeSymbol yieldInstruction,
            INamedTypeSymbol customYieldInstruction, INamedTypeSymbol coroutine)
        {
            _compilation = compilation;
            _testMethods = testMethods;
            _enumerator = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);
            _yieldInstruction = yieldInstruction;
            _customYieldInstruction = customYieldInstruction;
            _coroutine = coroutine;
        }

        public void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, _enumerator)
                || !_testMethods.IsTestMethod(method))
            {
                return;
            }

            if (YieldsOnlyUnityInstructions(method, method.ContainingType, 0, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
            }
        }

        // A method with no yield statement hands over an enumerator built elsewhere, which the walk cannot inspect.
        // The depth bound also ends a recursive helper, so no in-progress set is needed.
        private bool YieldsOnlyUnityInstructions(IMethodSymbol method, INamedTypeSymbol fixture, int depth,
            CancellationToken cancellationToken)
        {
            if (depth > DepthBoundedWalker.MaxDepth
                || OperationAnalysis.MethodBody(_compilation, method, cancellationToken) is not { } body)
            {
                return false;
            }

            var yields = new List<IReturnOperation>();
            CollectYields(body, yields, cancellationToken);
            foreach (var yield in yields)
            {
                if (yield.ReturnedValue is { } returned
                    && !IsConvertible(OperationAnalysis.WithoutImplicitConversions(returned), fixture, depth,
                        cancellationToken))
                {
                    return false;
                }
            }

            return yields.Count > 0;
        }

        // Yields in a lambda or local function belong to that function, not to the method. A list is filled rather
        // than an iterator returned, so that the recursion allocates one object per body instead of one per node.
        private static void CollectYields(IOperation operation, List<IReturnOperation> yields,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (operation)
            {
                case IAnonymousFunctionOperation:
                case ILocalFunctionOperation:
                    return;
                case IReturnOperation { Kind: OperationKind.YieldReturn or OperationKind.YieldBreak } yield:
                    yields.Add(yield);
                    return;
            }

            foreach (var child in operation.ChildOperations)
            {
                CollectYields(child, yields, cancellationToken);
            }
        }

        private bool IsConvertible(IOperation yielded, INamedTypeSymbol fixture, int depth,
            CancellationToken cancellationToken)
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
                   && (SymbolEqualityComparer.Default.Equals(callee.ContainingType, fixture)
                       || ActionAttributeAnalysis.DerivesFrom(fixture, callee.ContainingType))
                   && YieldsOnlyUnityInstructions(callee, fixture, depth + 1, cancellationToken);
        }

        // The namespace is checked rather than the assembly: in tests the Unity types are dummies compiled into the
        // test assembly. Coroutine derives from YieldInstruction but stands for a coroutine of the code under test.
        private bool IsUnityInstruction(INamedTypeSymbol type)
        {
            return !SymbolEqualityComparer.Default.Equals(type, _coroutine)
                   && IsInUnityEngineNamespace(type)
                   && (ActionAttributeAnalysis.DerivesFrom(type, _yieldInstruction)
                       || ActionAttributeAnalysis.DerivesFrom(type, _customYieldInstruction));
        }

        // The namespace chain is walked instead of compared as a display string, which would allocate a string per
        // yielded type on every keystroke.
        private static bool IsInUnityEngineNamespace(INamedTypeSymbol type)
        {
            var ns = type.ContainingNamespace;
            while (ns is { IsGlobalNamespace: false })
            {
                if (ns.ContainingNamespace.IsGlobalNamespace)
                {
                    return string.Equals(ns.Name, "UnityEngine", StringComparison.Ordinal);
                }

                ns = ns.ContainingNamespace;
            }

            return false;
        }
    }
}
