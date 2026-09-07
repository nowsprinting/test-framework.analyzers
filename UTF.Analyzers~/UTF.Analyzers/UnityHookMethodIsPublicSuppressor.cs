using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF3004: Suppress NUnit1028 (The non-test method is public) on a method marked with UnitySetUp, UnityOneTimeSetUp,
/// UnityTearDown, or UnityOneTimeTearDown.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnityHookMethodIsPublicSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3004";
    public const string SuppressedDiagnosticId = "NUnit1028";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Method is a UnitySetUp, UnityOneTimeSetUp, UnityTearDown, or UnityOneTimeTearDown method.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
    }
}
