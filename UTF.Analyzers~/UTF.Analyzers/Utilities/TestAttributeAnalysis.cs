using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Recognizes a method marked with TestAttribute, TestCaseAttribute, or TestCaseSourceAttribute for the return-type rules
/// (UTF1002, UTF1006). TestMethodAnalysis (ITestBuilder-based) is not used because those rules commit to exactly these
/// three attributes: UnityTestAttribute also implements ITestBuilder and has its own return-type validation.
/// </summary>
internal sealed class TestAttributeAnalysis
{
    private readonly ImmutableArray<INamedTypeSymbol> _testAttributes;

    private TestAttributeAnalysis(ImmutableArray<INamedTypeSymbol> testAttributes)
    {
        _testAttributes = testAttributes;
    }

    public static TestAttributeAnalysis? TryCreate(Compilation compilation)
    {
        var testAttributes = new[]
            {
                "NUnit.Framework.TestAttribute", "NUnit.Framework.TestCaseAttribute",
                "NUnit.Framework.TestCaseSourceAttribute"
            }
            .Select(compilation.GetTypeByMetadataName)
            .OfType<INamedTypeSymbol>()
            .ToImmutableArray();
        return testAttributes.IsEmpty ? null : new TestAttributeAnalysis(testAttributes);
    }

    /// <summary>
    /// Plain loops rather than LINQ because this runs on every method symbol: Contains with a comparer boxes the
    /// ImmutableArray and Any allocates a closure per call.
    /// </summary>
    public bool IsTestMethod(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass?.OriginalDefinition;
            foreach (var testAttribute in _testAttributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attributeClass, testAttribute))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// The defect of a return-type rule is the return type, not any single attribute, so one diagnostic is reported at
    /// the return type even when several test attributes are applied to the method.
    /// </summary>
    public static Location ReturnTypeLocation(IMethodSymbol method, CancellationToken cancellationToken)
    {
        return (method.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax)
            ?.ReturnType.GetLocation() ?? method.Locations[0];
    }
}
