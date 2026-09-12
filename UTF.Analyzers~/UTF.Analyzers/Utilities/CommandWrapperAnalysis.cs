using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// The attributes implementing <c>NUnit.Framework.Interfaces.ICommandWrapper</c> that Unity Test Framework supports
/// on async and coroutine-style test methods. Shared by UTF1005 and UTF5002 so that the two rules agree on what a supported wrapper is.
/// </summary>
internal static class CommandWrapperAnalysis
{
    // Unity Test Framework substitutes the commands produced by the NUnit attributes by exact type name
    // (TryReplaceWithEnumerableCommand), and ParametrizedIgnoreAttribute produces an internal command that implements
    // IEnumerableTestMethodCommand. Callers therefore match by exact type as well; a derived attribute is reported.
    private static readonly string[] SupportedWrapperAttributeNames =
    {
        "NUnit.Framework.RepeatAttribute",
        "NUnit.Framework.RetryAttribute",
        "NUnit.Framework.MaxTimeAttribute",
        "UnityEngine.TestTools.ParametrizedIgnoreAttribute",
    };

    /// <summary>
    /// Resolves the supported wrapper attributes in <paramref name="compilation"/>. Types that are not referenced are omitted.
    /// </summary>
    public static ImmutableHashSet<ISymbol> SupportedWrapperAttributes(Compilation compilation)
    {
        return SupportedWrapperAttributeNames
            .Select(compilation.GetTypeByMetadataName)
            .OfType<ISymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);
    }
}
