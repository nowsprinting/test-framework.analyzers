using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF3006: Suppress VSTHRD200 (Use Async suffix for async methods) on a method marked with an attribute that implements
/// ITestBuilder or ISimpleTestBuilder, i.e. a method NUnit runs as a test.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncSuffixOnTestMethodSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3006";
    public const string SuppressedDiagnosticId = "VSTHRD200";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Method is a test method; its name is read in test reports, not by a caller deciding whether to await it.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // VSTHRD200 reports at methodSymbol.Locations[0] (the identifier) and skips overrides and interface implementations.
        // Local function reports fall through, and the add/remove direction is not examined: both share the ID and neither has a caller to inform.
        TestMethodAnalysis.ReportSuppressionsOnTestMethods(context, Rule);
    }
}
