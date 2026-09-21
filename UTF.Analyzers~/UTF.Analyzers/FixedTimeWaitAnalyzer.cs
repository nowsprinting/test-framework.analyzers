using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF4004: Waiting for a fixed time in test, setup, and teardown methods is not recommended.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FixedTimeWaitAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4004";

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
    }
}
