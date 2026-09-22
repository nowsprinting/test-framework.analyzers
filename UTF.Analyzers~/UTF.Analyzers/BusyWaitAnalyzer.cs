using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4003: Waiting for a condition without yielding or awaiting does not advance frames.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BusyWaitAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4003";

    // The BCL calls that only pass time. The list is closed: it does not grow with libraries or projects.
    private static readonly string[] ThreadPassTimeNames = { "Sleep", "Yield", "SpinWait" };

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Waiting for a condition without yielding or awaiting does not advance frames",
        messageFormat:
        "'{0}' waits for a condition without yielding or awaiting: the Editor freezes and a Timeout attribute cannot end the test. Yield or await while waiting instead.",
        category: "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test, setup, or teardown method that waits without returning control to the test runner: a while or do loop whose body does no work (empty, or only calls to Thread.Sleep, Thread.Yield, Thread.SpinWait, or SpinWait.SpinOnce), and any call to SpinWait.SpinUntil, directly in the method or in helpers, lambdas, and local functions up to two levels deep. Unity Test Framework runs tests on the main thread, so no frame advances while such a wait spins, a condition driven by the main thread never becomes true, and the Timeout attribute is never checked.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4003.md");

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
        var thread = compilation.GetTypeByMetadataName("System.Threading.Thread");
        var spinWait = compilation.GetTypeByMetadataName("System.Threading.SpinWait");
        if (thread is null || spinWait is null)
        {
            return;
        }

        var cache = new DepthBoundedWalker.CalleeCache();
        TestOrHookMethodWalk.Register(context, Rule,
            token => new Walker(compilation, cache, thread, spinWait, token));
    }

    private sealed class Walker : DepthBoundedWalker
    {
        private readonly INamedTypeSymbol _thread;
        private readonly INamedTypeSymbol _spinWait;

        public Walker(Compilation compilation, CalleeCache cache, INamedTypeSymbol thread,
            INamedTypeSymbol spinWait, CancellationToken cancellationToken)
            : base(compilation, cache, cancellationToken)
        {
            _thread = thread;
            _spinWait = spinWait;
        }

        // A wait found in a callee stays reported at the wait, unlike WaitAnalysis, because the fix is applied there.
        protected override bool TryMatch(IOperation operation)
        {
            switch (operation)
            {
                case IWhileLoopOperation loop when DoesNoWork(loop.Body):
                    Found.Add((OperationAnalysis.LoopKeyword(loop), loop.ConditionIsTop ? "while" : "do"));
                    return true;
                case IInvocationOperation invocation when IsSpinUntil(invocation.TargetMethod):
                    Found.Add((operation.Syntax.GetLocation(), "SpinWait.SpinUntil"));
                    return true;
                default:
                    return false;
            }
        }

        // A single-statement body ("while (!x) Thread.Sleep(10);") is the statement itself, not a block.
        private bool DoesNoWork(IOperation body)
        {
            switch (body)
            {
                case IEmptyOperation:
                    return true;
                case IBlockOperation block:
                    foreach (var statement in block.Operations)
                    {
                        if (!DoesNoWork(statement))
                        {
                            return false;
                        }
                    }

                    return true;
                case IExpressionStatementOperation { Operation: IInvocationOperation { TargetMethod: { } method } }:
                    return PassesTime(method);
                default:
                    return false;
            }
        }

        private bool PassesTime(IMethodSymbol method)
        {
            var type = method.ContainingType;
            return (SymbolEqualityComparer.Default.Equals(type, _thread)
                    && Array.IndexOf(ThreadPassTimeNames, method.Name) >= 0)
                   || (SymbolEqualityComparer.Default.Equals(type, _spinWait)
                       && string.Equals(method.Name, "SpinOnce", StringComparison.Ordinal));
        }

        private bool IsSpinUntil(IMethodSymbol method)
        {
            return SymbolEqualityComparer.Default.Equals(method.ContainingType, _spinWait)
                   && string.Equals(method.Name, "SpinUntil", StringComparison.Ordinal);
        }
    }
}
