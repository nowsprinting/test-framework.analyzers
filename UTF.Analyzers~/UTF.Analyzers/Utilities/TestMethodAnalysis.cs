using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Recognizes a test method the way NUnit's DefaultTestCaseBuilder.CanBuildFrom does: a method is a test method when one
/// of its attributes implements ITestBuilder or ISimpleTestBuilder. A list of well-known attribute names is not used
/// because it would miss attributes derived from them and user-defined builders, which NUnit runs as tests all the same.
/// </summary>
internal sealed class TestMethodAnalysis
{
    private readonly INamedTypeSymbol _testBuilder;
    private readonly INamedTypeSymbol _simpleTestBuilder;

    private TestMethodAnalysis(INamedTypeSymbol testBuilder, INamedTypeSymbol simpleTestBuilder)
    {
        _testBuilder = testBuilder;
        _simpleTestBuilder = simpleTestBuilder;
    }

    public static TestMethodAnalysis? TryCreate(Compilation compilation)
    {
        var testBuilder = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ITestBuilder");
        var simpleTestBuilder = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ISimpleTestBuilder");
        return testBuilder is null || simpleTestBuilder is null
            ? null
            : new TestMethodAnalysis(testBuilder, simpleTestBuilder);
    }

    public bool IsTestMethod(IMethodSymbol method)
    {
        return method.GetAttributes().Any(a => a.AttributeClass is { } c
                                               && (c.AllInterfaces.Contains(_testBuilder, SymbolEqualityComparer.Default)
                                                   || c.AllInterfaces.Contains(_simpleTestBuilder,
                                                       SymbolEqualityComparer.Default)));
    }

    /// <summary>
    /// Suppresses every reported diagnostic located at the identifier of a test method. The suppressed rule must report at
    /// the method identifier and must skip overrides itself: reports on other nodes (local functions, types, fields,
    /// parameters) fall through, and overridden declarations are not walked.
    /// </summary>
    public static void ReportSuppressionsOnTestMethods(SuppressionAnalysisContext context, SuppressionDescriptor rule)
    {
        var testMethods = TryCreate(context.Compilation);
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

            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            var method = context.GetSemanticModel(tree).GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
            if (method is IMethodSymbol m && testMethods.IsTestMethod(m))
            {
                context.ReportSuppression(Suppression.Create(rule, diagnostic));
            }
        }
    }
}
