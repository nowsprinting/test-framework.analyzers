using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    // Deepest helper body that is walked; the test method body is depth 0, as in DepthBoundedWalker.
    private const int MaxDepth = 2;

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
        var enumerator = compilation.GetTypeByMetadataName("System.Collections.IEnumerator");
        var yieldInstruction = compilation.GetTypeByMetadataName("UnityEngine.YieldInstruction");
        var customYieldInstruction = compilation.GetTypeByMetadataName("UnityEngine.CustomYieldInstruction");
        if (testMethods is null || enumerator is null || yieldInstruction is null || customYieldInstruction is null)
        {
            return;
        }

        var coroutine = compilation.GetTypeByMetadataName("UnityEngine.Coroutine");
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

    private sealed class Walk
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _yieldInstruction;
        private readonly INamedTypeSymbol _customYieldInstruction;
        private readonly INamedTypeSymbol? _coroutine;
        private readonly INamedTypeSymbol _fixture;
        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<IMethodSymbol> _inProgress = new(SymbolEqualityComparer.Default);

        public Walk(Compilation compilation, INamedTypeSymbol yieldInstruction,
            INamedTypeSymbol customYieldInstruction, INamedTypeSymbol? coroutine, INamedTypeSymbol fixture,
            CancellationToken cancellationToken)
        {
            _compilation = compilation;
            _yieldInstruction = yieldInstruction;
            _customYieldInstruction = customYieldInstruction;
            _coroutine = coroutine;
            _fixture = fixture;
            _cancellationToken = cancellationToken;
        }

        // Anything the walk cannot see through counts as not convertible, the opposite of DepthBoundedWalker, because
        // the report requires every yield to be convertible; that is why the shared walker is not reused here.
        public bool YieldsOnlyUnityInstructions(IMethodSymbol method, int depth)
        {
            if (depth > MaxDepth || !_inProgress.Add(method))
            {
                return false;
            }

            try
            {
                var body = OperationAnalysis.MethodBody(_compilation, method, _cancellationToken);
                return body is not null && IsIterator(body) && Yields(body).All(y => IsConvertible(y, depth));
            }
            finally
            {
                _inProgress.Remove(method);
            }
        }

        // A method that returns an enumerator built elsewhere hands over a coroutine the walk cannot inspect.
        private static bool IsIterator(IOperation body)
        {
            return body.Syntax.DescendantNodes(n => n is not AnonymousFunctionExpressionSyntax
                                                    && n is not LocalFunctionStatementSyntax)
                .OfType<YieldStatementSyntax>().Any();
        }

        // Yields in a lambda or local function belong to that function, not to the method.
        private IEnumerable<IOperation> Yields(IOperation operation)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            switch (operation)
            {
                case IAnonymousFunctionOperation:
                case ILocalFunctionOperation:
                    yield break;
                case IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }:
                    yield return OperationAnalysis.WithoutImplicitConversions(returned);
                    yield break;
            }

            foreach (var child in operation.ChildOperations)
            {
                foreach (var yielded in Yields(child))
                {
                    yield return yielded;
                }
            }
        }

        private bool IsConvertible(IOperation yielded, int depth)
        {
            if (yielded is ILiteralOperation { ConstantValue: { HasValue: true, Value: null } })
            {
                return true;
            }

            if (yielded.Type is { } type && IsUnityInstruction(type))
            {
                return true;
            }

            // A helper on any other type, including a test-only MonoBehaviour, is the code under test.
            return yielded is IInvocationOperation invocation
                   && IsDeclaredOnFixture(invocation.TargetMethod.OriginalDefinition)
                   && YieldsOnlyUnityInstructions(invocation.TargetMethod.OriginalDefinition, depth + 1);
        }

        private bool IsDeclaredOnFixture(IMethodSymbol method)
        {
            for (var type = _fixture; type is not null; type = type.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
                {
                    return true;
                }
            }

            return false;
        }

        // The namespace is checked rather than the assembly: in tests the Unity types are dummies compiled into the
        // test assembly. Coroutine derives from YieldInstruction but stands for a coroutine of the code under test.
        private bool IsUnityInstruction(ITypeSymbol type)
        {
            if (SymbolEqualityComparer.Default.Equals(type, _coroutine) || !IsInUnityEngineNamespace(type))
            {
                return false;
            }

            for (var t = type; t is not null; t = t.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(t, _yieldInstruction)
                    || SymbolEqualityComparer.Default.Equals(t, _customYieldInstruction))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsInUnityEngineNamespace(ITypeSymbol type)
        {
            var ns = type.ContainingNamespace;
            while (ns is { IsGlobalNamespace: false })
            {
                if (ns.ContainingNamespace is { IsGlobalNamespace: true })
                {
                    return ns.Name == "UnityEngine";
                }

                ns = ns.ContainingNamespace;
            }

            return false;
        }
    }
}
