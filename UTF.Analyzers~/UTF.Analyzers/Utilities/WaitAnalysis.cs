using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Finds the waits in a method that end only when a condition changes: loops that yield or await, and predicate-wait
/// APIs. Helper methods, lambdas, and local functions are followed two levels deep. Shared by UTF4001 and UTF4002.
/// </summary>
internal sealed class WaitAnalysis
{
    /// <summary>
    /// Deepest body that is walked; the analyzed method body is depth 0.
    /// </summary>
    private const int MaxDepth = 2;

    private static readonly string[] UniTaskWaitNames =
        { "WaitUntil", "WaitWhile", "WaitUntilValueChanged", "WaitUntilCanceled" };

    private static readonly string[] UniTaskTimeoutNames = { "Timeout", "TimeoutWithoutException" };

    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol? _waitUntil;
    private readonly INamedTypeSymbol? _waitWhile;
    private readonly INamedTypeSymbol? _uniTask;
    private readonly INamedTypeSymbol? _uniTaskExtensions;
    private readonly INamedTypeSymbol? _timeSpan;

    public WaitAnalysis(Compilation compilation)
    {
        _compilation = compilation;
        _waitUntil = compilation.GetTypeByMetadataName("UnityEngine.WaitUntil");
        _waitWhile = compilation.GetTypeByMetadataName("UnityEngine.WaitWhile");
        _uniTask = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTask");
        _uniTaskExtensions = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTaskExtensions");
        _timeSpan = compilation.GetTypeByMetadataName("System.TimeSpan");
    }

    /// <summary>
    /// The waits in <paramref name="method"/>, with their reported location and name. A UniTask predicate wait that
    /// is the receiver of Timeout(...) or TimeoutWithoutException(...) is returned with HasTimeoutChain set; whether
    /// that counts as bounded is the caller's policy.
    /// </summary>
    public IEnumerable<(Location Location, string Name, bool HasTimeoutChain)> Waits(IMethodSymbol method,
        CancellationToken cancellationToken)
    {
        if (OperationAnalysis.MethodBody(_compilation, method, cancellationToken) is not { } body)
        {
            return Array.Empty<(Location, string, bool)>();
        }

        var walker = new Walker(this, cancellationToken);
        walker.Visit(body, 0);
        return walker.Found;
    }

    private bool IsPredicateWait(IMethodSymbol method)
    {
        return SymbolEqualityComparer.Default.Equals(method.ContainingType, _uniTask)
               && Array.IndexOf(UniTaskWaitNames, method.Name) >= 0;
    }

    // The extension call appears as an invocation whose Instance is the wait when bound in reduced form; the
    // instance is checked rather than the argument list so that an explicit UniTaskExtensions.Timeout(wait, ...)
    // call is still recognized through the first argument.
    private bool IsTimeoutReceiver(IInvocationOperation wait)
    {
        var parent = wait.Parent;
        while (parent is IConversionOperation { IsImplicit: true } or IArgumentOperation)
        {
            parent = parent.Parent;
        }

        return parent is IInvocationOperation { TargetMethod: { } outer }
               && SymbolEqualityComparer.Default.Equals(outer.ContainingType, _uniTaskExtensions)
               && Array.IndexOf(UniTaskTimeoutNames, outer.Name) >= 0;
    }

    // The overloads that take a TimeSpan timeout end by themselves.
    private bool IsPredicateYieldInstruction(IMethodSymbol constructor)
    {
        var type = constructor.ContainingType;
        if (!SymbolEqualityComparer.Default.Equals(type, _waitUntil)
            && !SymbolEqualityComparer.Default.Equals(type, _waitWhile))
        {
            return false;
        }

        foreach (var parameter in constructor.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, _timeSpan))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class Walker
    {
        private readonly WaitAnalysis _analysis;
        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<IMethodSymbol> _inProgress = new(SymbolEqualityComparer.Default);

        public List<(Location Location, string Name, bool HasTimeoutChain)> Found { get; } = new();

        public Walker(WaitAnalysis analysis, CancellationToken cancellationToken)
        {
            _analysis = analysis;
            _cancellationToken = cancellationToken;
        }

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
                case IWhileLoopOperation loop when YieldsOrAwaits(loop.Body):
                    Found.Add((LoopKeyword(loop), loop.ConditionIsTop ? "while" : "do", false));
                    return;
                case IInvocationOperation invocation when _analysis.IsPredicateWait(invocation.TargetMethod):
                    Found.Add((operation.Syntax.GetLocation(),
                        $"{invocation.TargetMethod.ContainingType.Name}.{invocation.TargetMethod.Name}",
                        _analysis.IsTimeoutReceiver(invocation)));
                    return;
                case IObjectCreationOperation { Constructor: { } constructor }
                    when _analysis.IsPredicateYieldInstruction(constructor):
                    Found.Add((operation.Syntax.GetLocation(), constructor.ContainingType.Name, false));
                    return;
                case IAwaitOperation { Operation: IInvocationOperation awaited }:
                    VisitCallee(awaited, depth);
                    break;
                // The yielded IEnumerator is wrapped in an implicit conversion to object.
                case IReturnOperation { Kind: OperationKind.YieldReturn, ReturnedValue: { } returned }
                    when OperationAnalysis.WithoutImplicitConversions(returned) is IInvocationOperation yielded:
                    VisitCallee(yielded, depth);
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
        private void VisitCallee(IInvocationOperation invocation, int depth)
        {
            var callee = invocation.TargetMethod.OriginalDefinition;
            if (depth >= MaxDepth
                || callee.MethodKind == MethodKind.LocalFunction
                || callee.DeclaringSyntaxReferences.IsEmpty
                || !_inProgress.Add(callee))
            {
                return;
            }

            // A wait inside the callee is reported once, at the call site, whatever it is and wherever it is. The call
            // site is bounded only when every wait inside is.
            var before = Found.Count;
            if (OperationAnalysis.MethodBody(_analysis._compilation, callee, _cancellationToken) is { } body)
            {
                Visit(body, depth + 1);
            }

            _inProgress.Remove(callee);
            if (Found.Count > before)
            {
                var allChained = true;
                for (var i = before; i < Found.Count; i++)
                {
                    allChained &= Found[i].HasTimeoutChain;
                }

                Found.RemoveRange(before, Found.Count - before);
                Found.Add((invocation.Syntax.GetLocation(), callee.Name, allChained));
            }
        }

        // A yield or await inside a lambda or local function belongs to that body, not to the loop.
        private static bool YieldsOrAwaits(IOperation operation)
        {
            switch (operation)
            {
                case IAnonymousFunctionOperation:
                case ILocalFunctionOperation:
                    return false;
                case IAwaitOperation:
                case IReturnOperation { Kind: OperationKind.YieldReturn }:
                    return true;
            }

            foreach (var child in operation.ChildOperations)
            {
                if (YieldsOrAwaits(child))
                {
                    return true;
                }
            }

            return false;
        }

        private static Location LoopKeyword(IWhileLoopOperation loop)
        {
            return loop.Syntax switch
            {
                WhileStatementSyntax w => w.WhileKeyword.GetLocation(),
                DoStatementSyntax d => d.DoKeyword.GetLocation(),
                var s => s.GetLocation(),
            };
        }
    }
}
