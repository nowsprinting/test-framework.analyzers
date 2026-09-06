using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Shared by UTF2002 and UTF2003: locating the delegate and constraint arguments of an NUnit assertion call,
/// deciding whether the delegate is async, and recognising a constraint expression rooted in Throws.
/// The two rules must agree on both checks so that every async delegate passed to Assert.That lands in exactly one of them.
/// </summary>
internal static class AsyncDelegateAnalysis
{
    /// <summary>
    /// Arguments are located by parameter type rather than by position so that the message overloads
    /// (del, expr, message, args) and Assert.Throws(Type, TestDelegate) are matched the same way.
    /// </summary>
    public static IArgumentOperation? DelegateArgument(IInvocationOperation invocation) =>
        invocation.Arguments.FirstOrDefault(a => a.Parameter?.Type.TypeKind == TypeKind.Delegate);

    public static IOperation? ConstraintArgument(IInvocationOperation invocation, INamedTypeSymbol resolveConstraint)
    {
        foreach (var a in invocation.Arguments)
        {
            if (SymbolEqualityComparer.Default.Equals(a.Parameter?.Type, resolveConstraint))
            {
                return a.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// The delegate counts as async when its return type is awaitable or its creation target has the async modifier.
    /// NUnit's own check (return type name starts with "System.Threading.Tasks.Task", or AsyncStateMachineAttribute) is
    /// deliberately not mirrored: a non-async lambda returning ValueTask, UniTask, or Awaitable slips past that check,
    /// so NUnit neither waits for it nor observes the exception thrown after the first await, and the test fails or
    /// passes for the wrong reason. The fix is the same in every case, so the rules report every awaitable.
    /// The return type is read from the bound delegate parameter (ActualValueDelegate&lt;Task&gt;) rather than from the
    /// lambda, so a delegate held in a variable is covered too; the async modifier must come from the creation target
    /// because it is the only trace of an async void lambda on a TestDelegate.
    /// </summary>
    public static bool IsAsyncDelegate(IArgumentOperation argument)
    {
        var invoke = ((INamedTypeSymbol)argument.Parameter!.Type).DelegateInvokeMethod;
        if (invoke is not null && IsAwaitable(invoke.ReturnType))
        {
            return true;
        }

        var value = argument.Value is IConversionOperation conversion ? conversion.Operand : argument.Value;
        return (value as IDelegateCreationOperation)?.Target switch
        {
            IAnonymousFunctionOperation lambda => lambda.Symbol.IsAsync,
            IMethodReferenceOperation reference => reference.Method.IsAsync,
            _ => false,
        };
    }

    /// <summary>
    /// The awaitable pattern is matched by member name because the language defines it that way; there is no symbol
    /// to compare against. Extension-method GetAwaiter is not resolved, which is a known limitation of the rules.
    /// </summary>
    private static bool IsAwaitable(ITypeSymbol type) =>
        type.GetMembers("GetAwaiter").OfType<IMethodSymbol>()
            .Any(m => !m.IsStatic && m.Parameters.IsEmpty && m.TypeParameters.IsEmpty);

    /// <summary>
    /// Walks a constraint expression such as Throws.TypeOf&lt;T&gt;().With.Message.EqualTo(...) back to its leftmost member
    /// and returns its display name ("Throws.TypeOf", "ThrowsConstraint") when that member belongs to Throws or is a
    /// ThrowsConstraint construction, or null otherwise.
    /// A local, field, parameter, or method result is not followed to its origin: following initializers needs a
    /// semantic model of the declaring file and still misses reassignments, and an unfollowed Throws constraint is
    /// reported by UTF2003 rather than going unreported.
    /// </summary>
    public static string? ThrowsRoot(IOperation? operation, INamedTypeSymbol throws,
        ImmutableHashSet<ISymbol> throwsConstraintTypes)
    {
        ISymbol? root = null;
        while (operation is not null)
        {
            switch (operation)
            {
                case IConversionOperation conversion:
                    operation = conversion.Operand;
                    break;
                case IInvocationOperation call:
                    root = call.TargetMethod;
                    operation = call.Instance;
                    break;
                case IPropertyReferenceOperation property:
                    root = property.Property;
                    operation = property.Instance;
                    break;
                case IObjectCreationOperation creation:
                    return creation.Type is not null && throwsConstraintTypes.Contains(creation.Type.OriginalDefinition)
                        ? creation.Type.Name
                        : null;
                default:
                    return null;
            }
        }

        return root is not null && SymbolEqualityComparer.Default.Equals(root.ContainingType, throws)
            ? $"{throws.Name}.{root.Name}"
            : null;
    }

    /// <summary>
    /// ThrowsNothingConstraint may be absent from an older NUnit; the set simply omits it.
    /// </summary>
    public static ImmutableHashSet<ISymbol> ThrowsConstraintTypes(Compilation compilation) =>
        new[]
            {
                "NUnit.Framework.Constraints.ThrowsConstraint", "NUnit.Framework.Constraints.ThrowsNothingConstraint",
            }
            .Select(compilation.GetTypeByMetadataName)
            .OfType<ISymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);
}
