using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        var timeout = compilation.GetTypeByMetadataName("NUnit.Framework.TimeoutAttribute");
        if (timeout is null || HasTimeout(compilation.Assembly, timeout))
        {
            return;
        }

        var testMethods = TestMethodAnalysis.TryCreate(compilation);
        if (testMethods is null)
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
        foreach (var attribute in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass?.OriginalDefinition, timeout))
            {
                return true;
            }
        }

        return false;
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

        private static readonly string[] UniTaskWaitNames =
            { "WaitUntil", "WaitWhile", "WaitUntilValueChanged", "WaitUntilCanceled" };

        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol? _waitUntil;
        private readonly INamedTypeSymbol? _waitWhile;
        private readonly INamedTypeSymbol? _uniTask;
        private readonly INamedTypeSymbol? _timeSpan;

        public WaitAnalysis(Compilation compilation)
        {
            _compilation = compilation;
            _waitUntil = compilation.GetTypeByMetadataName("UnityEngine.WaitUntil");
            _waitWhile = compilation.GetTypeByMetadataName("UnityEngine.WaitWhile");
            _uniTask = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTask");
            _timeSpan = compilation.GetTypeByMetadataName("System.TimeSpan");
        }

        public IEnumerable<(Location Location, string Name)> UnboundedWaits(IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            var walker = new Walker(this, cancellationToken);
            if (OperationAnalysis.MethodBody(_compilation, method, cancellationToken) is { } body)
            {
                walker.Visit(body, 0);
            }

            return walker.Waits;
        }

        private bool IsPredicateWait(IMethodSymbol method)
        {
            return SymbolEqualityComparer.Default.Equals(method.ContainingType, _uniTask)
                   && Array.IndexOf(UniTaskWaitNames, method.Name) >= 0;
        }

        // The overloads that take a TimeSpan timeout end by themselves.
        private bool IsPredicateYieldInstruction(IMethodSymbol constructor)
        {
            var type = constructor.ContainingType;
            if (!SymbolEqualityComparer.Default.Equals(type, _waitUntil)
                && !SymbolEqualityComparer.Default.Equals(type, _waitWhile))
            {
                return false;
            }

            foreach (var parameter in constructor.Parameters)
            {
                if (SymbolEqualityComparer.Default.Equals(parameter.Type, _timeSpan))
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class Walker
        {
            private readonly WaitAnalysis _analysis;
            private readonly CancellationToken _cancellationToken;
            private readonly HashSet<IMethodSymbol> _inProgress = new(SymbolEqualityComparer.Default);

            public List<(Location Location, string Name)> Waits { get; } = new();

            public Walker(WaitAnalysis analysis, CancellationToken cancellationToken)
            {
                _analysis = analysis;
                _cancellationToken = cancellationToken;
            }

            public void Visit(IOperation operation, int depth)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                switch (operation)
                {
                    case IAnonymousFunctionOperation lambda:
                        VisitNested(lambda.Body, depth);
                        return;
                    case ILocalFunctionOperation { Body: { } body }:
                        VisitNested(body, depth);
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
                        VisitCallee(awaited, depth);
                        break;
                    // The yielded IEnumerator is wrapped in an implicit conversion to object.
                    case IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }
                        when OperationAnalysis.WithoutImplicitConversions(returned) is IInvocationOperation yielded:
                        VisitCallee(yielded, depth);
                        break;
                }

                foreach (var child in operation.ChildOperations)
                {
                    Visit(child, depth);
                }
            }

            private void VisitNested(IOperation body, int depth)
            {
                if (depth < MaxDepth)
                {
                    Visit(body, depth + 1);
                }
            }

            // A local function is walked where it is declared, so following its invocation would report it twice.
            // Results are not cached across call sites, as in UTF5001: the walk is bounded by MaxDepth instead.
            private void VisitCallee(IInvocationOperation invocation, int depth)
            {
                var callee = invocation.TargetMethod.OriginalDefinition;
                if (depth >= MaxDepth
                    || callee.MethodKind == MethodKind.LocalFunction
                    || callee.DeclaringSyntaxReferences.IsEmpty
                    || !_inProgress.Add(callee))
                {
                    return;
                }

                // A wait inside the callee is reported once, at the call site, whatever it is and wherever it is.
                var before = Waits.Count;
                if (OperationAnalysis.MethodBody(_analysis._compilation, callee, _cancellationToken) is { } body)
                {
                    Visit(body, depth + 1);
                }

                _inProgress.Remove(callee);
                if (Waits.Count > before)
                {
                    Waits.RemoveRange(before, Waits.Count - before);
                    Waits.Add((invocation.Syntax.GetLocation(), callee.Name));
                }
            }

            // A yield or await inside a lambda or local function belongs to that body, not to the loop.
            private static bool YieldsOrAwaits(IOperation operation)
            {
                switch (operation)
                {
                    case IAnonymousFunctionOperation:
                    case ILocalFunctionOperation:
                        return false;
                    case IAwaitOperation:
                    case IReturnOperation { Kind: OperationKind.YieldReturn }:
                        return true;
                }

                foreach (var child in operation.ChildOperations)
                {
                    if (YieldsOrAwaits(child))
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
