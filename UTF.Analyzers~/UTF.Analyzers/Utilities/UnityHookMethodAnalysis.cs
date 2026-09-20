using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

internal static class UnityHookMethodAnalysis
{
    /// <summary>
    /// Returns whether <paramref name="method"/> is marked with any of <paramref name="attributes"/>, on itself or on a
    /// base declaration it overrides. A null entry stands for an attribute absent from the compilation and never matches.
    /// </summary>
    public static bool HasAnyAttribute(IMethodSymbol? method, params INamedTypeSymbol?[] attributes)
    {
        return FindAttribute(method, attributes) is not null;
    }

    /// <summary>
    /// The first of <paramref name="attributes"/> that marks <paramref name="method"/>, on itself or on a base
    /// declaration it overrides, or null when none does.
    /// </summary>
    public static INamedTypeSymbol? FindAttribute(IMethodSymbol? method, params INamedTypeSymbol?[] attributes)
    {
        for (; method is not null; method = method.OverriddenMethod)
        {
            foreach (var attribute in method.GetAttributes())
            {
                var attributeClass = attribute.AttributeClass?.OriginalDefinition;
                foreach (var candidate in attributes)
                {
                    if (candidate is not null && SymbolEqualityComparer.Default.Equals(attributeClass, candidate))
                    {
                        return candidate;
                    }
                }
            }
        }

        return null;
    }
}
