using System;
using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Shared by UTF5003 and UTF5004: which attribute classes implement an action interface, and the AttributeUsage the compiler applies to them.
/// </summary>
internal static class ActionAttributeAnalysis
{
    public static bool DerivesFrom(INamedTypeSymbol symbol, INamedTypeSymbol baseType)
    {
        for (var type = symbol.BaseType; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsOrDerivesFrom(INamedTypeSymbol candidate, INamedTypeSymbol interfaceType)
    {
        return SymbolEqualityComparer.Default.Equals(candidate, interfaceType) || Implements(candidate, interfaceType);
    }

    public static bool Implements(INamedTypeSymbol symbol, INamedTypeSymbol interfaceType)
    {
        // foreach instead of the LINQ Contains overload: every named type and base list entry in the compilation reaches
        // here, and the LINQ overload boxes the ImmutableArray on each call.
        foreach (var inherited in symbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(inherited, interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether the class's own base list (any declaration) names the interface or an interface derived from it.
    /// </summary>
    public static bool NamesInterface(INamedTypeSymbol symbol, INamedTypeSymbol interfaceType)
    {
        foreach (var declared in symbol.Interfaces)
        {
            if (IsOrDerivesFrom(declared, interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The AttributeUsage the compiler applies: the class's own, else the nearest base class's, else All.
    /// Roslyn's GetAttributeUsageInfo is internal, so the base chain is walked here.
    /// </summary>
    public static AttributeTargets EffectiveValidOn(INamedTypeSymbol symbol, INamedTypeSymbol attributeUsage)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            foreach (var data in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attributeUsage)
                    && data.ConstructorArguments.Length == 1
                    && data.ConstructorArguments[0].Value is int validOn)
                {
                    return (AttributeTargets)validOn;
                }
            }
        }

        return AttributeTargets.All;
    }
}
