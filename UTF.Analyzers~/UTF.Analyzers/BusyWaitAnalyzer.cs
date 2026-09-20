using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF4003: Waiting for a condition without yielding or awaiting does not advance frames.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BusyWaitAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4003";

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
    }
}
