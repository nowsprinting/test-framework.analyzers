using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Walks a method body and the bodies it reaches through lambdas, local functions, and the invocations that are the
/// operand of await or yield return, two levels deep. A subclass decides which operations count and how a wait
/// found inside a callee is reported. Shared by <see cref="WaitAnalysis"/> (UTF4001, UTF4002) and UTF4003.
/// </summary>
internal abstract class DepthBoundedWalker
{
    /// <summary>
    /// Deepest body that is walked; the analyzed method body is depth 0.
    /// </summary>
    internal const int MaxDepth = 2;

    /// <summary>
    /// What a walk of a callee body added to <see cref="Found"/>, shared by every walker of a compilation so that a
    /// helper yielded by many test methods is walked once. The result depends on the depth the callee is reached at,
    /// since it bounds how far the walk follows the callee's own calls, so there is one map per depth.
    /// </summary>
    internal sealed class CalleeCache
    {
        private readonly ConcurrentDictionary<IMethodSymbol, ImmutableArray<(Location Location, string Name)>>[]
            _byDepth = new ConcurrentDictionary<IMethodSymbol, ImmutableArray<(Location, string)>>[MaxDepth + 1];

        public CalleeCache()
        {
            for (var depth = 1; depth <= MaxDepth; depth++)
            {
                _byDepth[depth] = new(SymbolEqualityComparer.Default);
            }
        }

        public ConcurrentDictionary<IMethodSymbol, ImmutableArray<(Location Location, string Name)>> this[int depth]
            => _byDepth[depth];
    }

    private readonly Compilation _compilation;
    private readonly CalleeCache _cache;
    private readonly CancellationToken _cancellationToken;

    public List<(Location Location, string Name)> Found { get; } = new();

    protected DepthBoundedWalker(Compilation compilation, CalleeCache cache, CancellationToken cancellationToken)
    {
        _compilation = compilation;
        _cache = cache;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Records <paramref name="operation"/> in <see cref="Found"/> when it is a wait, and returns whether it was
    /// handled; a handled operation's children are not walked.
    /// </summary>
    protected abstract bool TryMatch(IOperation operation);

    public void Visit(IOperation operation, int depth)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        switch (operation)
        {
            case IAnonymousFunctionOperation lambda:
                VisitNested(lambda.Body, depth);
                return;
            case ILocalFunctionOperation { Body: { } body }:
                VisitNested(body, depth);
                return;
            case IAwaitOperation { Operation: IInvocationOperation awaited }:
                VisitCallee(awaited, depth);
                break;
            // The yielded IEnumerator is wrapped in an implicit conversion to object.
            case IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }
                when OperationAnalysis.WithoutImplicitConversions(returned) is IInvocationOperation yielded:
                VisitCallee(yielded, depth);
                break;
            default:
                if (TryMatch(operation))
                {
                    return;
                }

                break;
        }

        foreach (var child in operation.ChildOperations)
        {
            Visit(child, depth);
        }
    }

    private void VisitNested(IOperation body, int depth)
    {
        if (depth < MaxDepth)
        {
            Visit(body, depth + 1);
        }
    }

    // A local function is walked where it is declared, so following its invocation would report it twice.
    // A helper that yields itself is not guarded against: the depth bound ends the recursion, and the waits found
    // again one level down land on the same locations as the first walk.
    protected virtual void VisitCallee(IInvocationOperation invocation, int depth)
    {
        var callee = invocation.TargetMethod.OriginalDefinition;
        if (depth >= MaxDepth
            || callee.MethodKind == MethodKind.LocalFunction
            || callee.DeclaringSyntaxReferences.IsEmpty)
        {
            return;
        }

        var cache = _cache[depth + 1];
        if (cache.TryGetValue(callee, out var cached))
        {
            Found.AddRange(cached);
            return;
        }

        var before = Found.Count;
        if (OperationAnalysis.MethodBody(_compilation, callee, _cancellationToken) is { } body)
        {
            Visit(body, depth + 1);
        }

        cache.TryAdd(callee, ImmutableArray.CreateRange(Found.GetRange(before, Found.Count - before)));
    }
}
