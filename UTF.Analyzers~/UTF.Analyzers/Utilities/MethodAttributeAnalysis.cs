using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Recognizes a method marked with one of an explicit set of attributes, for the return-type rules (UTF1002, UTF1006,
/// UTF1007). TestMethodAnalysis (ITestBuilder-based) is not used because those rules commit to exactly the named
/// attributes: UnityTestAttribute also implements ITestBuilder and has its own return-type validation.
/// </summary>
internal sealed class MethodAttributeAnalysis
{
    public static readonly ImmutableArray<string> TestAttributes = ImmutableArray.Create(
        "NUnit.Framework.TestAttribute", "NUnit.Framework.TestCaseAttribute",
        "NUnit.Framework.TestCaseSourceAttribute");

    public static readonly ImmutableArray<string> SetUpTearDownAttributes = ImmutableArray.Create(
        "NUnit.Framework.SetUpAttribute", "NUnit.Framework.TearDownAttribute");

    private readonly ImmutableArray<INamedTypeSymbol> _attributes;

    private MethodAttributeAnalysis(ImmutableArray<INamedTypeSymbol> attributes)
    {
        _attributes = attributes;
    }

    /// <summary>Returns null when none of <paramref name="metadataNames"/> resolves in the compilation.</summary>
    public static MethodAttributeAnalysis? TryCreate(Compilation compilation, ImmutableArray<string> metadataNames)
    {
        var attributes = metadataNames
            .Select(compilation.GetTypeByMetadataName)
            .OfType<INamedTypeSymbol>()
            .ToImmutableArray();
        return attributes.IsEmpty ? null : new MethodAttributeAnalysis(attributes);
    }

    /// <summary>
    /// Plain loops rather than LINQ because this runs on every method symbol: Contains with a comparer boxes the
    /// ImmutableArray and Any allocates a closure per call. UnityHookMethodAnalysis.FindAttribute is not reused because
    /// it walks OverriddenMethod; the return-type rules report only on the declaration that spells the return type.
    /// </summary>
    public bool HasAttribute(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass?.OriginalDefinition;
            foreach (var candidate in _attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attributeClass, candidate))
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
