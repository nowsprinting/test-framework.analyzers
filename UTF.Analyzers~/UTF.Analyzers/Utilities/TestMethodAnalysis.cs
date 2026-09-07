using System.Linq;
using Microsoft.CodeAnalysis;

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
}
