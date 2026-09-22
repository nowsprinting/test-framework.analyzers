using System;
using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

internal static class UnityHookMethodAnalysis
{
    private static readonly string[] AllHookAttributeNames =
    {
        "NUnit.Framework.SetUpAttribute",
        "NUnit.Framework.TearDownAttribute",
        "NUnit.Framework.OneTimeSetUpAttribute",
        "NUnit.Framework.OneTimeTearDownAttribute",
        "UnityEngine.TestTools.UnitySetUpAttribute",
        "UnityEngine.TestTools.UnityTearDownAttribute",
        "UnityEngine.TestTools.UnityOneTimeSetUpAttribute",
        "UnityEngine.TestTools.UnityOneTimeTearDownAttribute"
    };

    /// <summary>
    /// Resolves every NUnit and UTF setup and teardown attribute in <paramref name="compilation"/>; an absent
    /// attribute is null.
    /// </summary>
    public static INamedTypeSymbol?[] ResolveAllHookAttributes(Compilation compilation)
    {
        return Array.ConvertAll(AllHookAttributeNames, compilation.GetTypeByMetadataName);
    }

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
