using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF5001: ApplyToTest, ApplyToContext, and Wrap must not throw exceptions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionInAttributeHookAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "ApplyToTest, ApplyToContext, and Wrap must not throw exceptions",
        messageFormat: "'{0}' must not throw exceptions",
        category: "Extensions",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an exception that can escape from an implementation of IApplyToTest.ApplyToTest, IApplyToContext.ApplyToContext, or ICommandWrapper.Wrap. Unity Test Framework invokes these methods outside its exception handling: an exception from ApplyToTest replaces the whole fixture with a single not-runnable entry that has no tests, and an exception from ApplyToContext on a test method or from Wrap aborts the entire test run.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5001.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
