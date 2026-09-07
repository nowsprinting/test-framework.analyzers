using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

internal static class UnityHookMethodAnalysis
{
    /// <summary>
    /// Returns whether <paramref name="method"/> is marked with either attribute, on itself or on a base declaration it overrides.
    /// </summary>
    public static bool HasEitherAttribute(IMethodSymbol? method, INamedTypeSymbol? first, INamedTypeSymbol? second)
    {
        for (; method is not null; method = method.OverriddenMethod)
        {
            foreach (var attribute in method.GetAttributes())
            {
                var attributeClass = attribute.AttributeClass?.OriginalDefinition;
                if (SymbolEqualityComparer.Default.Equals(attributeClass, first) ||
                    SymbolEqualityComparer.Default.Equals(attributeClass, second))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
