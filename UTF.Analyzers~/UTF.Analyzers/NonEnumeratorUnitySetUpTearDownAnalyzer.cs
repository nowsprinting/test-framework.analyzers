using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1009: Only IEnumerator is supported as the return type of methods with the UnitySetUp, UnityTearDown,
/// UnityOneTimeSetUp, and UnityOneTimeTearDown attributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonEnumeratorUnitySetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1009";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Only IEnumerator is supported as the return type of methods with the UnitySetUp, UnityTearDown, UnityOneTimeSetUp, and UnityOneTimeTearDown attributes",
        messageFormat:
        "'{0}' is not supported as the return type of a method marked with '{1}': the method never runs. Return 'IEnumerator' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with UnitySetUpAttribute, UnityTearDownAttribute, UnityOneTimeSetUpAttribute, or UnityOneTimeTearDownAttribute whose return type is not System.Collections.IEnumerator. Unity Test Framework does not recognize such a method as a setup or teardown method, so it never runs, and the tests run without it and report no error.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1009.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
