using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Classifies an NUnit assertion call that receives an async delegate into the rule that owns it.
/// UTF2002 and UTF2003 partition those calls between them, and a single classifier keeps the partition exhaustive:
/// each analyzer only asks "is this mine" instead of maintaining its own negation of the other rule's condition.
/// </summary>
internal sealed class AsyncDelegateAnalysis
{
    public enum Owner
    {
        None,

        /// <summary>UTF2002: a Throws constraint at the call site, or Assert.Throws, Assert.Catch, Assert.DoesNotThrow.</summary>
        ThrowsConstraint,

        /// <summary>UTF2003: Assert.That with any other constraint, or Assume.That with any constraint.</summary>
        ConstraintModel,
    }

    private readonly INamedTypeSymbol _assert;
    private readonly INamedTypeSymbol _throws;
    private readonly INamedTypeSymbol _resolveConstraint;
    private readonly ImmutableHashSet<ISymbol> _throwsConstraintTypes;
    private readonly ImmutableHashSet<ISymbol> _that;
    private readonly ImmutableHashSet<ISymbol> _testDelegateAssertions;

    private AsyncDelegateAnalysis(INamedTypeSymbol assert, INamedTypeSymbol assume, INamedTypeSymbol throws,
        INamedTypeSymbol resolveConstraint, ImmutableHashSet<ISymbol> throwsConstraintTypes)
    {
        _assert = assert;
        _throws = throws;
        _resolveConstraint = resolveConstraint;
        _throwsConstraintTypes = throwsConstraintTypes;
        _that = Methods(assert, "That").Union(Methods(assume, "That"));
        _testDelegateAssertions = Methods(assert, "Throws", "Catch", "DoesNotThrow");
    }

    public static AsyncDelegateAnalysis? TryCreate(Compilation compilation)
    {
        var assert = compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        var assume = compilation.GetTypeByMetadataName("NUnit.Framework.Assume");
        var throws = compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
        var resolveConstraint = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
        if (assert is null || assume is null || throws is null || resolveConstraint is null)
        {
            return null;
        }

        // ThrowsNothingConstraint may be absent from an older NUnit; the set simply omits it.
        var throwsConstraintTypes = new[]
            {
                "NUnit.Framework.Constraints.ThrowsConstraint", "NUnit.Framework.Constraints.ThrowsNothingConstraint",
            }
            .Select(compilation.GetTypeByMetadataName)
            .OfType<ISymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);
        return new AsyncDelegateAnalysis(assert, assume, throws, resolveConstraint, throwsConstraintTypes);
    }

    /// <summary>
    /// Returns the owning rule of <paramref name="invocation"/>, with the async delegate argument to report at and the
    /// display name of the API that receives it ("Throws.TypeOf", "ThrowsConstraint", "Assert.Throws", "Assume.That").
    /// </summary>
    public Owner Classify(IInvocationOperation invocation, out IArgumentOperation argument, out string receivingApi)
    {
        argument = null!;
        receivingApi = string.Empty;
        var method = invocation.TargetMethod.OriginalDefinition;
        var isThat = _that.Contains(method);
        if (!isThat && !_testDelegateAssertions.Contains(method))
        {
            return Owner.None;
        }

        // The delegate is checked before the constraint because it is the cheaper and more selective filter:
        // most Assert.That calls take a plain value, and most Assert.Throws calls take a synchronous lambda.
        var delegateArgument = DelegateArgument(invocation);
        if (delegateArgument is null || !IsAsyncDelegate(delegateArgument))
        {
            return Owner.None;
        }

        argument = delegateArgument;
        // The name is built from the symbol rather than the syntax so that a call through "using static" still reads "Throws.X".
        receivingApi = $"{method.ContainingType.Name}.{method.Name}";
        if (!isThat)
        {
            return Owner.ThrowsConstraint;
        }

        // Assume.That is not split on its constraint: UTF2002's try/catch message targets Assert only, so every
        // async delegate passed to Assume.That belongs to UTF2003, Throws constraints included.
        if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, _assert))
        {
            return Owner.ConstraintModel;
        }

        var root = ThrowsRoot(ConstraintArgument(invocation));
        if (root is null)
        {
            return Owner.ConstraintModel;
        }

        receivingApi = root;
        return Owner.ThrowsConstraint;
    }

    private static ImmutableHashSet<ISymbol> Methods(INamedTypeSymbol type, params string[] names) =>
        names.SelectMany(name => type.GetMembers(name).OfType<IMethodSymbol>())
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

    /// <summary>
    /// Arguments are located by parameter type rather than by position so that the message overloads
    /// (del, expr, message, args) and Assert.Throws(Type, TestDelegate) are matched the same way.
    /// </summary>
    private static IArgumentOperation? DelegateArgument(IInvocationOperation invocation)
    {
        foreach (var a in invocation.Arguments)
        {
            if (a.Parameter?.Type.TypeKind == TypeKind.Delegate)
            {
                return a;
            }
        }

        return null;
    }

    private IOperation? ConstraintArgument(IInvocationOperation invocation)
    {
        foreach (var a in invocation.Arguments)
        {
            if (SymbolEqualityComparer.Default.Equals(a.Parameter?.Type, _resolveConstraint))
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
    private static bool IsAsyncDelegate(IArgumentOperation argument)
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
    private static bool IsAwaitable(ITypeSymbol type)
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

    /// <summary>
    /// Walks a constraint expression such as Throws.TypeOf&lt;T&gt;().With.Message.EqualTo(...) back to its leftmost member
    /// and returns its display name ("Throws.TypeOf", "ThrowsConstraint") when that member belongs to Throws or is a
    /// ThrowsConstraint construction, or null otherwise.
    /// A local, field, parameter, or method result is not followed to its origin: following initializers needs a
    /// semantic model of the declaring file and still misses reassignments, and an unfollowed Throws constraint is
    /// reported by UTF2003 rather than going unreported.
    /// </summary>
    private string? ThrowsRoot(IOperation? operation)
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
                    return creation.Type is not null &&
                           _throwsConstraintTypes.Contains(creation.Type.OriginalDefinition)
                        ? creation.Type.Name
                        : null;
                default:
                    return null;
            }
        }

        return root is not null && SymbolEqualityComparer.Default.Equals(root.ContainingType, _throws)
            ? $"{_throws.Name}.{root.Name}"
            : null;
    }
}
