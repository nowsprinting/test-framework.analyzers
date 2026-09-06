using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF3002: Suppress CS8618 (Non-nullable field or property is uninitialized) when the member is initialized
/// in a UnitySetUp or UnityOneTimeSetUp method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonNullableMemberInitializedInUnitySetUpSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3002";
    public const string SuppressedDiagnosticId = "CS8618";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Field/Property is initialized in UnitySetUp or UnityOneTimeSetUp method.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
    }
}
