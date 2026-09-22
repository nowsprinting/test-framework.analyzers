using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Decides whether a coroutine yields only null and Unity yield instructions, so that it converts to an async Task
/// method mechanically (UTF4006 for test methods, UTF4007 for UnitySetUp and UnityTearDown methods).
/// </summary>
// DepthBoundedWalker is not reused: it descends into lambdas and local functions, whereas a yield in a lambda
// belongs to the lambda and must not count for the method; and an operand it cannot see through must exempt the
// method here rather than go unreported. One instance serves the whole compilation, so the per-method state
// (fixture, cancellation token) is passed along instead of stored.
internal sealed class UnityYieldWalk
{
    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol _enumerator;
    private readonly INamedTypeSymbol _yieldInstruction;
    private readonly INamedTypeSymbol _customYieldInstruction;
    private readonly INamedTypeSymbol _coroutine;

    // Whether a helper reached at depth 1 or 2 yields only Unity instructions, shared by every method of the
    // compilation. The fixture the walk started from is not part of the key, so that a base-class helper is
    // walked once for all the fixtures deriving from it; the helpers it yields on its own type or its bases pass
    // the fixture check for every one of them.
    // ponytail: a base helper that yields a method of a derived fixture resolves for whichever fixture is
    // analyzed first, add the fixture to the key if anyone writes one.
    private readonly ConcurrentDictionary<IMethodSymbol, bool>[] _helperResults =
        new ConcurrentDictionary<IMethodSymbol, bool>[DepthBoundedWalker.MaxDepth + 1];

    private UnityYieldWalk(Compilation compilation, INamedTypeSymbol yieldInstruction,
        INamedTypeSymbol customYieldInstruction, INamedTypeSymbol coroutine)
    {
        _compilation = compilation;
        _enumerator = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);
        _yieldInstruction = yieldInstruction;
        _customYieldInstruction = customYieldInstruction;
        _coroutine = coroutine;
        for (var depth = 1; depth <= DepthBoundedWalker.MaxDepth; depth++)
        {
            _helperResults[depth] = new ConcurrentDictionary<IMethodSymbol, bool>(SymbolEqualityComparer.Default);
        }
    }

    /// <summary>Null when the Unity yield instruction types are absent from the compilation.</summary>
    public static UnityYieldWalk? TryCreate(Compilation compilation)
    {
        var yieldInstruction = compilation.GetTypeByMetadataName("UnityEngine.YieldInstruction");
        var customYieldInstruction = compilation.GetTypeByMetadataName("UnityEngine.CustomYieldInstruction");
        var coroutine = compilation.GetTypeByMetadataName("UnityEngine.Coroutine");
        return yieldInstruction is null || customYieldInstruction is null || coroutine is null
            ? null
            : new UnityYieldWalk(compilation, yieldInstruction, customYieldInstruction, coroutine);
    }

    /// <summary>
    /// Whether <paramref name="method"/> returns the non-generic IEnumerator and every yield return in it yields null
    /// or a Unity yield instruction, directly or through iterator helpers declared on its containing type or a base.
    /// </summary>
    public bool IsConvertibleCoroutine(IMethodSymbol method, CancellationToken cancellationToken)
    {
        return SymbolEqualityComparer.Default.Equals(method.ReturnType, _enumerator)
               && YieldsOnlyUnityInstructions(method, method.ContainingType, 0, cancellationToken);
    }

    // The return type is included in the span because it is what the fix changes, together with the name.
    public static Location SignatureLocation(IMethodSymbol method, CancellationToken cancellationToken)
    {
        if (method.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not MethodDeclarationSyntax
            declaration)
        {
            return method.Locations[0];
        }

        return Location.Create(declaration.SyntaxTree,
            TextSpan.FromBounds(declaration.ReturnType.SpanStart, declaration.Identifier.Span.End));
    }

    // A method with no yield statement hands over an enumerator built elsewhere, which the walk cannot inspect.
    // The depth bound also ends a recursive helper, so no in-progress set is needed.
    private bool YieldsOnlyUnityInstructions(IMethodSymbol method, INamedTypeSymbol fixture, int depth,
        CancellationToken cancellationToken)
    {
        if (depth > DepthBoundedWalker.MaxDepth)
        {
            return false;
        }

        if (depth == 0)
        {
            return WalkYields(method, fixture, depth, cancellationToken);
        }

        var results = _helperResults[depth];
        return results.TryGetValue(method, out var cached)
            ? cached
            : results.GetOrAdd(method, WalkYields(method, fixture, depth, cancellationToken));
    }

    private bool WalkYields(IMethodSymbol method, INamedTypeSymbol fixture, int depth,
        CancellationToken cancellationToken)
    {
        if (OperationAnalysis.MethodBody(_compilation, method, cancellationToken) is not { } body)
        {
            return false;
        }

        var yields = new List<IReturnOperation>();
        CollectYields(body, yields, cancellationToken);
        foreach (var yield in yields)
        {
            if (yield.ReturnedValue is { } returned
                && !IsConvertible(OperationAnalysis.WithoutImplicitConversions(returned), fixture, depth,
                    cancellationToken))
            {
                return false;
            }
        }

        return yields.Count > 0;
    }

    // Yields in a lambda or local function belong to that function, not to the method. A list is filled rather
    // than an iterator returned, so that the recursion allocates one object per body instead of one per node.
    private static void CollectYields(IOperation operation, List<IReturnOperation> yields,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (operation)
        {
            case IAnonymousFunctionOperation:
            case ILocalFunctionOperation:
                return;
            case IReturnOperation { Kind: OperationKind.YieldReturn or OperationKind.YieldBreak } yield:
                yields.Add(yield);
                return;
        }

        foreach (var child in operation.ChildOperations)
        {
            CollectYields(child, yields, cancellationToken);
        }
    }

    private bool IsConvertible(IOperation yielded, INamedTypeSymbol fixture, int depth,
        CancellationToken cancellationToken)
    {
        if (yielded is ILiteralOperation { ConstantValue: { HasValue: true, Value: null } })
        {
            return true;
        }

        if (yielded.Type is INamedTypeSymbol type && IsUnityInstruction(type))
        {
            return true;
        }

        // A helper on any other type, including a test-only MonoBehaviour, is the code under test.
        var callee = yielded is IInvocationOperation invocation ? invocation.TargetMethod.OriginalDefinition : null;
        return callee is not null
               && (SymbolEqualityComparer.Default.Equals(callee.ContainingType, fixture)
                   || ActionAttributeAnalysis.DerivesFrom(fixture, callee.ContainingType))
               && YieldsOnlyUnityInstructions(callee, fixture, depth + 1, cancellationToken);
    }

    // The namespace is checked rather than the assembly: in tests the Unity types are dummies compiled into the
    // test assembly. Coroutine derives from YieldInstruction but stands for a coroutine of the code under test.
    private bool IsUnityInstruction(INamedTypeSymbol type)
    {
        return !SymbolEqualityComparer.Default.Equals(type, _coroutine)
               && IsInUnityEngineNamespace(type)
               && (ActionAttributeAnalysis.DerivesFrom(type, _yieldInstruction)
                   || ActionAttributeAnalysis.DerivesFrom(type, _customYieldInstruction));
    }

    // The namespace chain is walked instead of compared as a display string, which would allocate a string per
    // yielded type on every keystroke.
    private static bool IsInUnityEngineNamespace(INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace;
        while (ns is { IsGlobalNamespace: false })
        {
            if (ns.ContainingNamespace.IsGlobalNamespace)
            {
                return string.Equals(ns.Name, "UnityEngine", StringComparison.Ordinal);
            }

            ns = ns.ContainingNamespace;
        }

        return false;
    }
}
