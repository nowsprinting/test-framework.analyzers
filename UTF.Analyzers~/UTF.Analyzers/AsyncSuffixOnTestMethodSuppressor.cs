using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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
    }
}
