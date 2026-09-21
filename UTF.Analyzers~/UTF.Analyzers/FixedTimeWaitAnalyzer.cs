using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4004: Waiting for a fixed time in test, setup, and teardown methods is not recommended.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FixedTimeWaitAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4004";

    // Yield instructions that complete after a duration, recognized by the type of the yielded value so that an
    // instruction cached in a field is seen as well as an inline object creation.
    private static readonly string[] YieldInstructionTypeNames =
    {
        "UnityEngine.WaitForSeconds", "UnityEngine.WaitForSecondsRealtime"
    };

    // Methods that complete after a duration, as (containing type, method name). Every overload of a name is a
    // fixed wait: a CancellationToken, PlayerLoopTiming, DelayType, or ignoreTimeScale argument changes which clock
    // measures the duration, not that the duration is fixed.
    private static readonly (string Type, string Method)[] MethodNames =
    {
        ("System.Threading.Tasks.Task", "Delay"),
        ("System.Threading.Thread", "Sleep"),
        ("UnityEngine.Awaitable", "WaitForSecondsAsync"),
        ("Cysharp.Threading.Tasks.UniTask", "Delay"),
        ("Cysharp.Threading.Tasks.UniTask", "WaitForSeconds")
    };

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Waiting for a fixed time in test, setup, and teardown methods is not recommended",
        messageFormat:
        "'{0}' waits for a fixed time: the test fails when the environment is slower than expected and wastes time when it is faster. Wait for the condition instead.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a test, setup, or teardown method that waits for a fixed time instead of for a condition: a yield return of a WaitForSeconds or WaitForSecondsRealtime, an await of UniTask.Delay, UniTask.WaitForSeconds, Task.Delay, or Awaitable.WaitForSecondsAsync, and a call to Thread.Sleep, directly in the method or in helpers, lambdas, and local functions up to two levels deep. A fixed wait is tuned to the author's machine, so the test fails when the environment is slower and wastes the surplus when it is faster.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4004.md");

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
        if (testMethods is null)
        {
            return;
        }

        var yieldInstructions = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var name in YieldInstructionTypeNames)
        {
            if (compilation.GetTypeByMetadataName(name) is { } type)
            {
                yieldInstructions.Add(type);
            }
        }

        // A handful of entries; a linear scan per invocation is cheaper than a set with a tuple comparer.
        var methods = ImmutableArray.CreateBuilder<(INamedTypeSymbol Type, string Name)>();
        foreach (var (typeName, method) in MethodNames)
        {
            if (compilation.GetTypeByMetadataName(typeName) is { } type)
            {
                methods.Add((type, method));
            }
        }

        var analysis = new CompilationAnalysis(compilation, testMethods,
            UnityHookMethodAnalysis.ResolveAllHookAttributes(compilation), yieldInstructions.ToImmutable(),
            methods.ToImmutable());
        context.RegisterSymbolAction(analysis.AnalyzeMethod, SymbolKind.Method);
    }

    private sealed class CompilationAnalysis
    {
        private readonly Compilation _compilation;
        private readonly TestMethodAnalysis _testMethods;
        private readonly INamedTypeSymbol?[] _hooks;
        private readonly ImmutableHashSet<INamedTypeSymbol> _yieldInstructions;
        private readonly ImmutableArray<(INamedTypeSymbol Type, string Name)> _methods;

        // A wait is reported at the wait, so a helper reached from several test methods would be reported once per
        // walk; the set keeps the first report only, as in UTF4003.
        private readonly ConcurrentDictionary<Location, bool> _reported = new();

        public CompilationAnalysis(Compilation compilation, TestMethodAnalysis testMethods,
            INamedTypeSymbol?[] hooks, ImmutableHashSet<INamedTypeSymbol> yieldInstructions,
            ImmutableArray<(INamedTypeSymbol Type, string Name)> methods)
        {
            _compilation = compilation;
            _testMethods = testMethods;
            _hooks = hooks;
            _yieldInstructions = yieldInstructions;
            _methods = methods;
        }

        private bool IsFixedWait(IMethodSymbol method)
        {
            foreach (var (type, name) in _methods)
            {
                if (SymbolEqualityComparer.Default.Equals(method.ContainingType, type)
                    && string.Equals(method.Name, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            if (!_testMethods.IsTestMethod(method) && !UnityHookMethodAnalysis.HasAnyAttribute(method, _hooks))
            {
                return;
            }

            if (OperationAnalysis.MethodBody(_compilation, method, context.CancellationToken) is not { } body)
            {
                return;
            }

            var walker = new Walker(this, context.CancellationToken);
            walker.Visit(body, 0);
            foreach (var (location, name) in walker.Found)
            {
                if (_reported.TryAdd(location, true))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, name));
                }
            }
        }

        private sealed class Walker : DepthBoundedWalker
        {
            private readonly CompilationAnalysis _analysis;

            public Walker(CompilationAnalysis analysis, CancellationToken cancellationToken)
                : base(analysis._compilation, cancellationToken)
            {
                _analysis = analysis;
            }

            // A wait found in a callee stays reported at the wait, as in UTF4003, because the fix is applied there.
            protected override bool TryMatch(IOperation operation)
            {
                switch (operation)
                {
                    case IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }
                        when OperationAnalysis.WithoutImplicitConversions(returned) is { Type: INamedTypeSymbol type } value
                             && _analysis._yieldInstructions.Contains(type):
                        if (!IsPollingInterval(operation))
                        {
                            Found.Add((value.Syntax.GetLocation(), type.Name));
                        }

                        return true;
                    case IInvocationOperation { TargetMethod: { } method } when _analysis.IsFixedWait(method):
                        if (!IsPollingInterval(operation))
                        {
                            Found.Add((operation.Syntax.GetLocation(), $"{method.ContainingType.Name}.{method.Name}"));
                        }

                        return true;
                    default:
                        return false;
                }
            }

            // A fixed wait inside a while or do loop is the polling interval of a wait for a condition, which UTF4001,
            // UTF4002, and UTF4003 cover. The ancestor chain is checked instead of tracking loop nesting in the walk,
            // so that the shared walker needs no loop state; the chain stops at the body being walked, so a lambda or
            // local function declared in the loop body is not excluded.
            private static bool IsPollingInterval(IOperation operation)
            {
                for (var parent = operation.Parent; parent is not null; parent = parent.Parent)
                {
                    switch (parent)
                    {
                        case IWhileLoopOperation:
                            return true;
                        case IAnonymousFunctionOperation:
                        case ILocalFunctionOperation:
                            return false;
                    }
                }

                return false;
            }
        }
    }
}
