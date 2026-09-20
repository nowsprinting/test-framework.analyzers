using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF4002: Waits for a condition in UnitySetUp, UnityTearDown, UnityOneTimeSetUp, UnityOneTimeTearDown, SetUp, and
/// TearDown methods must have a time limit.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnboundedWaitInSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Waits for a condition in UnitySetUp, UnityTearDown, UnityOneTimeSetUp, UnityOneTimeTearDown, SetUp, and TearDown methods must have a time limit",
        messageFormat:
        "'{0}' waits for a condition without a time limit in a method marked with '{1}': the test run hangs when the condition never holds, and a Timeout attribute does not interrupt it. Give the wait a time limit of its own, e.g. a WaitUntil with a TimeSpan timeout or a for loop.",
        category: "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a setup or teardown method that waits for a condition with no time limit of its own: a while or do loop that yields or awaits in its body, or a call to WaitUntil, WaitWhile, or the UniTask.WaitUntil family that is not bounded by a timeout, directly in the method or in helpers, lambdas, and local functions up to two levels deep. Unity Test Framework checks the Timeout attribute and its 180-second default only while the test method body runs, so when the condition never holds the test run hangs.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4002.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
