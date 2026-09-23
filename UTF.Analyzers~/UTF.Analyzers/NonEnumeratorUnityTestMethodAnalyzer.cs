using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1008: Only IEnumerator is supported as the return type of methods with the UnityTest attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonEnumeratorUnityTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1008";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only IEnumerator is supported as the return type of methods with the UnityTest attribute",
        messageFormat:
        "'{0}' is not supported as the return type of a UnityTest method: the test never runs. Return 'IEnumerator' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with UnityTestAttribute whose return type is not System.Collections.IEnumerator. Unity Test Framework marks such a test as not runnable, so it fails without running.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1008.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
    }
}
