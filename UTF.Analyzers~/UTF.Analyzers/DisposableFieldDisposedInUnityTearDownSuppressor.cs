using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF3003: Suppress CA1001 (Types that own disposable fields should be disposable) on a test fixture
/// that declares a UnityTearDown or UnityOneTimeTearDown method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableFieldDisposedInUnityTearDownSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3003";
    public const string SuppressedDiagnosticId = "CA1001";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Field should be Disposed in UnityTearDown or UnityOneTimeTearDown method.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
    }
}
