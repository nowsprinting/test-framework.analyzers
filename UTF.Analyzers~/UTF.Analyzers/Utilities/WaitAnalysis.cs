using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Finds the waits in a method that end only when a condition changes: loops that yield or await, and predicate-wait
/// APIs. Helper methods, lambdas, and local functions are followed two levels deep. Shared by UTF4001 and UTF4002.
/// </summary>
internal sealed class WaitAnalysis
{
    private static readonly string[] UniTaskWaitNames =
        { "WaitUntil", "WaitWhile", "WaitUntilValueChanged", "WaitUntilCanceled" };

    private static readonly string[] UniTaskTimeoutNames = { "Timeout", "TimeoutWithoutException" };

    // Types whose members are read to get the current time or frame. A loop whose condition reads one of them ends
    // when the deadline passes. The whole type is matched rather than each clock member (Time.time,
    // DateTime.UtcNow, Stopwatch.ElapsedMilliseconds, ...): the non-clock members (Time.timeScale, DateTime.Year)
    // make no sense in a wait condition, so a member list would only add maintenance.
    private static readonly string[] ClockTypeNames =
    {
        "UnityEngine.Time", "System.DateTime", "System.DateTimeOffset", "System.Diagnostics.Stopwatch"
    };

    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol? _waitUntil;
    private readonly INamedTypeSymbol? _waitWhile;
    private readonly INamedTypeSymbol? _uniTask;
    private readonly INamedTypeSymbol? _uniTaskExtensions;
    private readonly INamedTypeSymbol? _timeSpan;
    private readonly HashSet<INamedTypeSymbol> _clockTypes = new(SymbolEqualityComparer.Default);

    public WaitAnalysis(Compilation compilation)
    {
        _compilation = compilation;
        _waitUntil = compilation.GetTypeByMetadataName("UnityEngine.WaitUntil");
        _waitWhile = compilation.GetTypeByMetadataName("UnityEngine.WaitWhile");
        _uniTask = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTask");
        _uniTaskExtensions = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTaskExtensions");
        _timeSpan = compilation.GetTypeByMetadataName("System.TimeSpan");
        foreach (var name in ClockTypeNames)
        {
            if (compilation.GetTypeByMetadataName(name) is { } clock)
            {
                _clockTypes.Add(clock);
            }
        }
    }

    /// <summary>
    /// The unbounded waits in <paramref name="method"/>, with their reported location and name. A UniTask predicate
    /// wait that is the receiver of Timeout(...) or TimeoutWithoutException(...) is bounded and not returned.
    /// </summary>
    public IEnumerable<(Location Location, string Name)> Waits(IMethodSymbol method,
        CancellationToken cancellationToken)
    {
        if (OperationAnalysis.MethodBody(_compilation, method, cancellationToken) is not { } body)
        {
            return Array.Empty<(Location, string)>();
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

    // A loop is a deadline when its condition reads a clock, or when it compares a variable that the body advances
    // from a clock ("elapsed += Time.deltaTime"). The direction of the comparison and the shape of the condition
    // are not checked: "while (Time.time > start)" and "while (!_flag || Time.time < deadline)" are taken as
    // bounded, since no one writes them on purpose. The condition is null only in error scenarios; the loop is then
    // reported as usual.
    private bool IsDeadline(IWhileLoopOperation loop)
    {
        if (loop.Condition is not { } condition)
        {
            return false;
        }

        if (ReadsClock(condition))
        {
            return true;
        }

        var compared = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        CollectVariables(condition, compared);
        return compared.Count > 0 && AssignsClock(loop.Body, compared);
    }

    private static void CollectVariables(IOperation operation, HashSet<ISymbol> variables)
    {
        switch (operation)
        {
            case ILocalReferenceOperation local:
                variables.Add(local.Local);
                break;
            case IFieldReferenceOperation field:
                variables.Add(field.Field);
                break;
        }

        foreach (var child in operation.ChildOperations)
        {
            CollectVariables(child, variables);
        }
    }

    private bool AssignsClock(IOperation operation, HashSet<ISymbol> variables)
    {
        if (operation is IAssignmentOperation assignment && ReadsClock(assignment.Value))
        {
            ISymbol? target = assignment.Target switch
            {
                ILocalReferenceOperation local => local.Local,
                IFieldReferenceOperation field => field.Field,
                _ => null,
            };
            if (target is not null && variables.Contains(target))
            {
                return true;
            }
        }

        foreach (var child in operation.ChildOperations)
        {
            if (AssignsClock(child, variables))
            {
                return true;
            }
        }

        return false;
    }

    private bool ReadsClock(IOperation condition)
    {
        var member = condition switch
        {
            IMemberReferenceOperation reference => reference.Member,
            IInvocationOperation invocation => invocation.TargetMethod,
            _ => null,
        };
        if (member is not null && _clockTypes.Contains(member.ContainingType))
        {
            return true;
        }

        foreach (var child in condition.ChildOperations)
        {
            if (ReadsClock(child))
            {
                return true;
            }
        }

        return false;
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

    private sealed class Walker : DepthBoundedWalker
    {
        private readonly WaitAnalysis _analysis;

        public Walker(WaitAnalysis analysis, CancellationToken cancellationToken)
            : base(analysis._compilation, cancellationToken)
        {
            _analysis = analysis;
        }

        protected override bool TryMatch(IOperation operation)
        {
            switch (operation)
            {
                case IWhileLoopOperation loop when YieldsOrAwaits(loop.Body) && !_analysis.IsDeadline(loop):
                    Found.Add((OperationAnalysis.LoopKeyword(loop), loop.ConditionIsTop ? "while" : "do"));
                    return true;
                // A CancellationToken argument does not bound the wait: the test runner never cancels it.
                case IInvocationOperation invocation when _analysis.IsPredicateWait(invocation.TargetMethod):
                    if (!_analysis.IsTimeoutReceiver(invocation))
                    {
                        Found.Add((operation.Syntax.GetLocation(),
                            $"{invocation.TargetMethod.ContainingType.Name}.{invocation.TargetMethod.Name}"));
                    }

                    return true;
                case IObjectCreationOperation { Constructor: { } constructor }
                    when _analysis.IsPredicateYieldInstruction(constructor):
                    Found.Add((operation.Syntax.GetLocation(), constructor.ContainingType.Name));
                    return true;
                default:
                    return false;
            }
        }

        // A wait inside the callee is reported once, at the call site, whatever it is and wherever it is.
        protected override void VisitCallee(IInvocationOperation invocation, int depth)
        {
            var before = Found.Count;
            base.VisitCallee(invocation, depth);
            if (Found.Count > before)
            {
                Found.RemoveRange(before, Found.Count - before);
                Found.Add((invocation.Syntax.GetLocation(), invocation.TargetMethod.OriginalDefinition.Name));
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
    }
}
