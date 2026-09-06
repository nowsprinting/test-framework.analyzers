using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF2003: Async delegates are not supported as the actual value of the constraint model.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncDelegateInConstraintModelAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2003";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Async delegates are not supported as the actual value of the constraint model",
        messageFormat:
        "Async delegates are not supported as the actual value of '{0}'. Await the operation in an async test method and assert on its result instead.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an async delegate passed as the actual value of Assert.That with a constraint that is not a Throws constraint, or passed as the actual value of Assume.That with any constraint. NUnit evaluates the delegate and waits for the returned Task synchronously on the calling thread. Unity Test Framework runs tests on the main thread, so the Editor freezes.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2003.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
