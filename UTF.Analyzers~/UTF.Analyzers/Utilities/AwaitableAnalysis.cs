using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

internal static class AwaitableAnalysis
{
    /// <summary>
    /// The awaitable pattern is matched by member name because the language defines it that way; there is no symbol
    /// to compare against. Extension-method GetAwaiter is not resolved, which is a known limitation of the rules.
    /// </summary>
    public static bool IsAwaitable(ITypeSymbol type)
    {
        foreach (var member in type.GetMembers("GetAwaiter"))
        {
            if (member is IMethodSymbol { IsStatic: false, Parameters.IsEmpty: true, TypeParameters.IsEmpty: true })
            {
                return true;
            }
        }

        return false;
    }
}
