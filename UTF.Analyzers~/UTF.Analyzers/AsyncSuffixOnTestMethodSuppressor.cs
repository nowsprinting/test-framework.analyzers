using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        var testMethods = TestMethodAnalysis.TryCreate(context.Compilation);
        if (testMethods is null)
        {
            return;
        }

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var tree = diagnostic.Location.SourceTree;
            if (tree is null)
            {
                continue;
            }

            // VSTHRD200 reports at the symbol's location, i.e. the identifier, whose enclosing node is the method declaration.
            // Reports on local functions resolve to a local function statement and fall through; the add/remove direction is
            // not examined because both share the ID and neither has a caller to inform.
            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            var method = context.GetSemanticModel(tree).GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
            // Overrides are not walked: VSTHRD200 skips IsOverride symbols before reporting, so only the method's own attributes matter.
            if (method is IMethodSymbol m && testMethods.IsTestMethod(m))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }
}
