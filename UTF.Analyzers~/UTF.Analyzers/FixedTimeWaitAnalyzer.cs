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
/// UTF4004: Waiting for a fixed time in test, setup, and teardown methods is not recommended.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FixedTimeWaitAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4004";

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
        var waitForSeconds = compilation.GetTypeByMetadataName("UnityEngine.WaitForSeconds");
        var waitForSecondsRealtime = compilation.GetTypeByMetadataName("UnityEngine.WaitForSecondsRealtime");
        var methods = new List<(INamedTypeSymbol Type, string Name)>();
        foreach (var (typeName, method) in MethodNames)
        {
            if (compilation.GetTypeByMetadataName(typeName) is { } type)
            {
                methods.Add((type, method));
            }
        }

        TestOrHookMethodWalk.Register(context, Rule,
            token => new Walker(compilation, waitForSeconds, waitForSecondsRealtime, methods, token));
    }

    private sealed class Walker : DepthBoundedWalker
    {
        private readonly INamedTypeSymbol? _waitForSeconds;
        private readonly INamedTypeSymbol? _waitForSecondsRealtime;
        private readonly List<(INamedTypeSymbol Type, string Name)> _methods;

        public Walker(Compilation compilation, INamedTypeSymbol? waitForSeconds,
            INamedTypeSymbol? waitForSecondsRealtime, List<(INamedTypeSymbol Type, string Name)> methods,
            CancellationToken cancellationToken)
            : base(compilation, cancellationToken)
        {
            _waitForSeconds = waitForSeconds;
            _waitForSecondsRealtime = waitForSecondsRealtime;
            _methods = methods;
        }

        // A wait found in a callee stays reported at the wait, as in UTF4003, because the fix is applied there.
        // A yield return is recognized by the type of the yielded value rather than by the object creation, so that
        // an instruction cached in a field is seen too.
        protected override bool TryMatch(IOperation operation)
        {
            (Location, string)? wait = operation switch
            {
                IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }
                    when OperationAnalysis.WithoutImplicitConversions(returned) is { Type: { } type } value
                         && IsFixedWaitInstruction(type)
                    => (value.Syntax.GetLocation(), type.Name),
                IInvocationOperation invocation when IsFixedWait(invocation.TargetMethod)
                    => (operation.Syntax.GetLocation(),
                        $"{invocation.TargetMethod.ContainingType.Name}.{invocation.TargetMethod.Name}"),
                _ => null,
            };
            if (wait is null)
            {
                return false;
            }

            if (!IsPollingInterval(operation))
            {
                Found.Add(wait.Value);
            }

            return true;
        }

        private bool IsFixedWaitInstruction(ITypeSymbol type)
        {
            return SymbolEqualityComparer.Default.Equals(type, _waitForSeconds)
                   || SymbolEqualityComparer.Default.Equals(type, _waitForSecondsRealtime);
        }

        // The name is compared first: it rejects almost every invocation before the symbol comparison.
        private bool IsFixedWait(IMethodSymbol method)
        {
            foreach (var (type, name) in _methods)
            {
                if (string.Equals(method.Name, name, StringComparison.Ordinal)
                    && SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
                {
                    return true;
                }
            }

            return false;
        }

        // A fixed wait inside a while or do loop is the polling interval of a wait for a condition, which UTF4001,
        // UTF4002, and UTF4003 cover. The ancestor chain is checked instead of tracking loop nesting in the walk, so
        // that the shared walker needs no loop state; the chain stops at the body being walked, so a lambda or local
        // function declared in the loop body is not excluded.
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
