using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF3005: Suppress CA1707 (Identifiers should not contain underscores) on a method marked with an attribute that
/// implements ITestBuilder or ISimpleTestBuilder, i.e. a method NUnit runs as a test.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnderscoreInTestMethodNameSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3005";
    public const string SuppressedDiagnosticId = "CA1707";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Method is a test method; underscores in its name separate the scenario and the expected result.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
    }
}
