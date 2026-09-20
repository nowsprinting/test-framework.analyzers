using System.Collections.Generic;
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
    protected const int MaxDepth = 2;

    private readonly Compilation _compilation;
    private readonly CancellationToken _cancellationToken;
    private readonly HashSet<IMethodSymbol> _inProgress = new(SymbolEqualityComparer.Default);

    public List<(Location Location, string Name)> Found { get; } = new();

    protected DepthBoundedWalker(Compilation compilation, CancellationToken cancellationToken)
    {
        _compilation = compilation;
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
    // Results are not cached across call sites, as in UTF5001: the walk is bounded by MaxDepth instead.
    protected virtual void VisitCallee(IInvocationOperation invocation, int depth)
    {
        var callee = invocation.TargetMethod.OriginalDefinition;
        if (depth >= MaxDepth
            || callee.MethodKind == MethodKind.LocalFunction
            || callee.DeclaringSyntaxReferences.IsEmpty
            || !_inProgress.Add(callee))
        {
            return;
        }

        if (OperationAnalysis.MethodBody(_compilation, callee, _cancellationToken) is { } body)
        {
            Visit(body, depth + 1);
        }

        _inProgress.Remove(callee);
    }
}
