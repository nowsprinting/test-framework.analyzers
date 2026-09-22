using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF2006: Property constraints, Ordered.By, and List.Map(...).Property look up properties that managed code stripping can remove.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StrippablePropertyLookupAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2006";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Property constraints, Ordered.By, and List.Map(...).Property look up properties that managed code stripping can remove",
        messageFormat:
        "'{0}' is looked up by name at run time and managed code stripping can remove it: the test fails in the Player. Read the property directly in the actual value instead.",
        category: "Assertion",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description:
        "Detects a property constraint (Has.Property, With.Property, Has.Length, Has.Count, Has.Message, Has.InnerException), Is.Ordered.By, and List.Map(...).Property whose property is not referenced by NUnit itself. NUnit finds the property with reflection at run time, so the Unity linker sees no reference to it and can remove it from a Player build.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF2006.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
