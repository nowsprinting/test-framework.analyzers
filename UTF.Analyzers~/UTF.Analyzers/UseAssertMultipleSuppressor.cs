using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF3001: Suppress NUnit2045 (Use Assert.Multiple) when Assert.Multiple is not available.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAssertMultipleSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3001";
    public const string SuppressedDiagnosticId = "NUnit2045";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Assert.Multiple is not available in the NUnit shipped with Unity Test Framework.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // Suppressing unconditionally would hide a valid suggestion for a project whose nunit.framework defines Assert.Multiple.
        var assert = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        if (assert is null || !assert.GetMembers("Multiple").IsEmpty)
        {
            return;
        }

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            context.ReportSuppression(Suppression.Create(Rule, diagnostic));
        }
    }
}
