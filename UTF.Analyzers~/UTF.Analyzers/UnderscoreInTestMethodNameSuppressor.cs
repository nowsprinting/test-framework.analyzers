using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        // Test methods are recognized through the builder interfaces rather than the four well-known attribute types:
        // NUnit's DefaultTestCaseBuilder.CanBuildFrom uses the same criterion, so derived and user-defined attributes are covered.
        var compilation = context.Compilation;
        var builders = new[]
            {
                "NUnit.Framework.Interfaces.ITestBuilder", "NUnit.Framework.Interfaces.ISimpleTestBuilder",
            }
            .Select(compilation.GetTypeByMetadataName)
            .OfType<ISymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);
        if (builders.IsEmpty)
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

            // CA1707 reports at the symbol's location, i.e. the method identifier, whose enclosing node is the method declaration.
            // Reports on types, fields, and parameters resolve to other nodes and fall through.
            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            var method = context.GetSemanticModel(tree).GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
            // Overrides are not walked: CA1707 skips IsOverride symbols before reporting, so only the method's own attributes matter.
            if (method is not null && method.GetAttributes().Any(a =>
                    a.AttributeClass is { } c && c.AllInterfaces.Any(i => builders.Contains(i.OriginalDefinition))))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }
}
