using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    /// <summary>
    /// Deepest body that is walked; the analyzed method body is depth 0, as in <see cref="WaitAnalysis"/>.
    /// </summary>
    private const int MaxDepth = 2;

    // The BCL calls that only pass time. The list is closed: it does not grow with libraries or projects.
    private static readonly string[] ThreadPassTimeNames = { "Sleep", "Yield", "SpinWait" };

    private static readonly string[] HookAttributeNames =
    {
        "NUnit.Framework.SetUpAttribute",
        "NUnit.Framework.TearDownAttribute",
        "NUnit.Framework.OneTimeSetUpAttribute",
        "NUnit.Framework.OneTimeTearDownAttribute",
        "UnityEngine.TestTools.UnitySetUpAttribute",
        "UnityEngine.TestTools.UnityTearDownAttribute",
        "UnityEngine.TestTools.UnityOneTimeSetUpAttribute",
        "UnityEngine.TestTools.UnityOneTimeTearDownAttribute"
    };

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
        var testMethods = TestMethodAnalysis.TryCreate(compilation);
        var thread = compilation.GetTypeByMetadataName("System.Threading.Thread");
        var spinWait = compilation.GetTypeByMetadataName("System.Threading.SpinWait");
        if (testMethods is null || thread is null || spinWait is null)
        {
            return;
        }

        var hooks = new INamedTypeSymbol?[HookAttributeNames.Length];
        for (var i = 0; i < hooks.Length; i++)
        {
            hooks[i] = compilation.GetTypeByMetadataName(HookAttributeNames[i]);
        }

        // A wait is reported at the loop, so a helper reached from several test methods, or a test method that is
        // also awaited by another one, would be reported once per walk; the set keeps the first report only.
        var reported = new ConcurrentDictionary<Location, bool>();
        context.RegisterSymbolAction(symbolContext =>
        {
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!testMethods.IsTestMethod(method) && !UnityHookMethodAnalysis.HasAnyAttribute(method, hooks))
            {
                return;
            }

            if (OperationAnalysis.MethodBody(compilation, method, symbolContext.CancellationToken) is not { } body)
            {
                return;
            }

            var walker = new Walker(compilation, thread, spinWait, symbolContext.CancellationToken);
            walker.Visit(body, 0);
            foreach (var (location, name) in walker.Found)
            {
                if (reported.TryAdd(location, true))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, name));
                }
            }
        }, SymbolKind.Method);
    }

    private sealed class Walker
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _thread;
        private readonly INamedTypeSymbol _spinWait;
        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<IMethodSymbol> _inProgress = new(SymbolEqualityComparer.Default);

        public List<(Location Location, string Name)> Found { get; } = new();

        public Walker(Compilation compilation, INamedTypeSymbol thread, INamedTypeSymbol spinWait,
            CancellationToken cancellationToken)
        {
            _compilation = compilation;
            _thread = thread;
            _spinWait = spinWait;
            _cancellationToken = cancellationToken;
        }

        // The walk follows the same edges as WaitAnalysis.Walker (lambdas, local functions, and await / yield return
        // operands, two levels deep) but reports inside the callee instead of collapsing to the call site, because
        // the fix is applied at the wait itself. The two walkers are kept separate rather than parameterized: the
        // callee handling is the only shared part, and a shared base would need a hook for each report policy.
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
                case IWhileLoopOperation loop when DoesNoWork(loop.Body):
                    Found.Add((OperationAnalysis.LoopKeyword(loop), loop.ConditionIsTop ? "while" : "do"));
                    return;
                case IInvocationOperation invocation when IsSpinUntil(invocation.TargetMethod):
                    Found.Add((operation.Syntax.GetLocation(), "SpinWait.SpinUntil"));
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

            if (OperationAnalysis.MethodBody(_compilation, callee, _cancellationToken) is { } body)
            {
                Visit(body, depth + 1);
            }

            _inProgress.Remove(callee);
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
                   || (SymbolEqualityComparer.Default.Equals(type, _spinWait) && method.Name == "SpinOnce");
        }

        private bool IsSpinUntil(IMethodSymbol method)
        {
            return SymbolEqualityComparer.Default.Equals(method.ContainingType, _spinWait)
                   && method.Name == "SpinUntil";
        }
    }
}
