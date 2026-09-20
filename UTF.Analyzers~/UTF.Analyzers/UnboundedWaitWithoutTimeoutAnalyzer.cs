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
/// UTF4001: Test methods that wait for a condition must have a Timeout attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnboundedWaitWithoutTimeoutAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Test methods that wait for a condition must have a Timeout attribute",
        messageFormat:
        "'{0}' waits for a condition without a time limit: the test runs for 180 seconds when the condition never holds. Apply a Timeout attribute with a short value to the test method.",
        category: "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method that waits for a condition with no time limit of its own, when neither the method, its containing type, nor the assembly has a Timeout attribute. A while or do loop that yields or awaits in its body, and a call to WaitUntil, WaitWhile, or the UniTask.WaitUntil family, are recognized, directly in the test method or in helpers, lambdas, and local functions up to two levels deep. When the condition never holds, such a test runs until the 180-second default timeout of Unity Test Framework.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4001.md");

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
        var timeout = compilation.GetTypeByMetadataName("NUnit.Framework.TimeoutAttribute");
        if (testMethods is null || timeout is null || HasTimeout(compilation.Assembly, timeout))
        {
            return;
        }

        var analysis = new WaitAnalysis(compilation);
        context.RegisterSymbolAction(symbolContext =>
        {
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!testMethods.IsTestMethod(method)
                || HasTimeout(method, timeout)
                || HasTimeout(method.ContainingType, timeout))
            {
                return;
            }

            foreach (var (location, name) in analysis.UnboundedWaits(method, symbolContext.CancellationToken))
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, name));
            }
        }, SymbolKind.Method);
    }

    // Only the symbol's own attributes are read: TimeoutAttribute is declared Inherited = false, so a Timeout on a
    // base class does not reach the fixture at run time.
    private static bool HasTimeout(ISymbol symbol, INamedTypeSymbol timeout)
    {
        return symbol.GetAttributes()
            .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass?.OriginalDefinition, timeout));
    }

    /// <summary>
    /// Finds the waits in a test method that end only when a condition changes: loops that yield or await, and
    /// predicate-wait APIs. Helper methods, lambdas, and local functions are followed two levels deep.
    /// </summary>
    private sealed class WaitAnalysis
    {
        /// <summary>
        /// Deepest body that is walked; the test method body is depth 0.
        /// </summary>
        private const int MaxDepth = 2;

        private readonly Compilation _compilation;
        private readonly ImmutableArray<INamedTypeSymbol> _predicateYieldInstructions;
        private readonly ImmutableArray<IMethodSymbol> _predicateWaits;

        public WaitAnalysis(Compilation compilation)
        {
            _compilation = compilation;
            _predicateYieldInstructions = new[] { "UnityEngine.WaitUntil", "UnityEngine.WaitWhile" }
                .Select(compilation.GetTypeByMetadataName)
                .OfType<INamedTypeSymbol>()
                .ToImmutableArray();
            var uniTask = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTask");
            _predicateWaits = uniTask is null
                ? ImmutableArray<IMethodSymbol>.Empty
                : new[] { "WaitUntil", "WaitWhile", "WaitUntilValueChanged", "WaitUntilCanceled" }
                    .SelectMany(name => uniTask.GetMembers(name))
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();
        }

        public IEnumerable<(Location Location, string Name)> UnboundedWaits(IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            var walker = new Walker(this, 0, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default),
                cancellationToken);
            walker.VisitBody(method);
            return walker.Waits;
        }

        private IOperation? Body(IMethodSymbol method, CancellationToken cancellationToken)
        {
            var syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
            return syntax is null
                ? null
                : _compilation.GetSemanticModel(syntax.SyntaxTree).GetOperation(syntax, cancellationToken);
        }

        private bool IsPredicateWait(IMethodSymbol method)
        {
            return _predicateWaits.Contains(method.OriginalDefinition, SymbolEqualityComparer.Default);
        }

        // The overloads that take a TimeSpan timeout end by themselves, so only the single-parameter constructor counts.
        private bool IsPredicateYieldInstruction(IMethodSymbol constructor)
        {
            return constructor.Parameters.Length == 1
                   && _predicateYieldInstructions.Contains(constructor.ContainingType, SymbolEqualityComparer.Default);
        }

        private sealed class Walker
        {
            private readonly WaitAnalysis _analysis;
            private readonly int _depth;
            private readonly HashSet<IMethodSymbol> _inProgress;
            private readonly CancellationToken _cancellationToken;

            public List<(Location Location, string Name)> Waits { get; } = new();

            public Walker(WaitAnalysis analysis, int depth, HashSet<IMethodSymbol> inProgress,
                CancellationToken cancellationToken)
            {
                _analysis = analysis;
                _depth = depth;
                _inProgress = inProgress;
                _cancellationToken = cancellationToken;
            }

            public void VisitBody(IMethodSymbol method)
            {
                if (_analysis.Body(method, _cancellationToken) is { } body)
                {
                    Visit(body);
                }
            }

            private void Visit(IOperation operation)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                switch (operation)
                {
                    case IAnonymousFunctionOperation lambda:
                        VisitNested(lambda.Body);
                        return;
                    case ILocalFunctionOperation localFunction:
                        VisitNested(localFunction.Body);
                        return;
                    case IWhileLoopOperation loop when YieldsOrAwaits(loop.Body):
                        Waits.Add((LoopKeyword(loop), loop.ConditionIsTop ? "while" : "do"));
                        return;
                    case IInvocationOperation invocation when _analysis.IsPredicateWait(invocation.TargetMethod):
                        Waits.Add((operation.Syntax.GetLocation(),
                            $"{invocation.TargetMethod.ContainingType.Name}.{invocation.TargetMethod.Name}"));
                        return;
                    case IObjectCreationOperation { Constructor: { } constructor }
                        when _analysis.IsPredicateYieldInstruction(constructor):
                        Waits.Add((operation.Syntax.GetLocation(), constructor.ContainingType.Name));
                        return;
                    case IAwaitOperation { Operation: IInvocationOperation awaited }:
                        VisitCallee(awaited);
                        break;
                    // The yielded IEnumerator is wrapped in an implicit conversion to object.
                    case IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }
                        when WithoutConversions(returned) is IInvocationOperation yielded:
                        VisitCallee(yielded);
                        break;
                }

                foreach (var child in operation.ChildOperations)
                {
                    Visit(child);
                }
            }

            private void VisitNested(IOperation? body)
            {
                if (body is null || _depth >= MaxDepth)
                {
                    return;
                }

                var nested = new Walker(_analysis, _depth + 1, _inProgress, _cancellationToken);
                nested.Visit(body);
                Waits.AddRange(nested.Waits);
            }

            // A local function is walked where it is declared, so following its invocation would report it twice.
            // Results are not cached across call sites, as in UTF5001: the walk is bounded by MaxDepth instead.
            private void VisitCallee(IInvocationOperation invocation)
            {
                var callee = invocation.TargetMethod.OriginalDefinition;
                if (_depth >= MaxDepth || callee.MethodKind == MethodKind.LocalFunction || !_inProgress.Add(callee))
                {
                    return;
                }

                var nested = new Walker(_analysis, _depth + 1, _inProgress, _cancellationToken);
                nested.VisitBody(callee);
                _inProgress.Remove(callee);
                if (nested.Waits.Count > 0)
                {
                    Waits.Add((invocation.Syntax.GetLocation(), callee.Name));
                }
            }

            private static IOperation WithoutConversions(IOperation operation)
            {
                while (operation is IConversionOperation { IsImplicit: true } conversion)
                {
                    operation = conversion.Operand;
                }

                return operation;
            }

            // A yield or await inside a lambda or local function belongs to that body, not to the loop.
            private static bool YieldsOrAwaits(IOperation body)
            {
                return body.DescendantsAndSelf()
                    .Any(o => o is IAwaitOperation or IReturnOperation { Kind: OperationKind.YieldReturn }
                              && !IsInsideFunction(o, body));
            }

            private static bool IsInsideFunction(IOperation operation, IOperation body)
            {
                for (var parent = operation.Parent; parent is not null && parent != body; parent = parent.Parent)
                {
                    if (parent is IAnonymousFunctionOperation or ILocalFunctionOperation)
                    {
                        return true;
                    }
                }

                return false;
            }

            private static Location LoopKeyword(IWhileLoopOperation loop)
            {
                return loop.Syntax switch
                {
                    WhileStatementSyntax w => w.WhileKeyword.GetLocation(),
                    DoStatementSyntax d => d.DoKeyword.GetLocation(),
                    var s => s.GetLocation(),
                };
            }
        }
    }
}
