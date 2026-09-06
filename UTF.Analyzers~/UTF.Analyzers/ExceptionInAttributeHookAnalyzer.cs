using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers;

/// <summary>
/// UTF5001: ApplyToTest, ApplyToContext, and Wrap must not throw exceptions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionInAttributeHookAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "ApplyToTest, ApplyToContext, and Wrap must not throw exceptions",
        messageFormat: "'{0}' must not throw exceptions",
        category: "Extensions",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an exception that can escape from an implementation of IApplyToTest.ApplyToTest, IApplyToContext.ApplyToContext, or ICommandWrapper.Wrap. Unity Test Framework invokes these methods outside its exception handling: an exception from ApplyToTest replaces the whole fixture with a single not-runnable entry that has no tests, and an exception from ApplyToContext on a test method or from Wrap aborts the entire test run.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5001.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        var exception = compilation.GetTypeByMetadataName("System.Exception");
        var hooks = new[]
            {
                ("NUnit.Framework.Interfaces.IApplyToTest", "ApplyToTest"),
                ("NUnit.Framework.Interfaces.IApplyToContext", "ApplyToContext"),
                ("NUnit.Framework.Interfaces.ICommandWrapper", "Wrap"),
            }
            .Select(hook => compilation.GetTypeByMetadataName(hook.Item1)?.GetMembers(hook.Item2).FirstOrDefault())
            .OfType<IMethodSymbol>()
            .ToImmutableArray();
        if (exception is null || hooks.IsEmpty)
        {
            return;
        }

        var analysis = new EscapeAnalysis(compilation, exception);
        // An implementation inherited from a base class that does not itself declare the interface is found through
        // the derived type, so the implementing method rather than the type is the unit of analysis; the same method
        // reached from several derived types is analyzed once.
        var analyzed = new ConcurrentDictionary<ISymbol, byte>(SymbolEqualityComparer.Default);

        context.RegisterSymbolAction(symbolContext =>
        {
            var type = (INamedTypeSymbol)symbolContext.Symbol;
            if (type.AllInterfaces.IsEmpty)
            {
                return;
            }

            foreach (var hook in hooks)
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                if (type.FindImplementationForInterfaceMember(hook) is not IMethodSymbol implementation)
                {
                    continue;
                }

                implementation = OverrideIn(type, implementation);
                if (!analyzed.TryAdd(implementation, 0))
                {
                    continue;
                }

                var member = $"{hook.ContainingType.Name}.{hook.Name}";
                foreach (var site in analysis.EscapingSites(implementation, symbolContext.CancellationToken))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, site.Syntax.GetLocation(), member));
                }
            }
        }, SymbolKind.NamedType);
    }

    // FindImplementationForInterfaceMember returns the method in the type that declares the interface even when
    // a derived type overrides it. Only the visited type's own override is looked up: an override in an intermediate
    // base class is found when that class itself is visited.
    private static IMethodSymbol OverrideIn(INamedTypeSymbol type, IMethodSymbol implementation)
    {
        implementation = implementation.OriginalDefinition;
        foreach (var candidate in type.GetMembers(implementation.Name).OfType<IMethodSymbol>())
        {
            for (var overridden = candidate.OverriddenMethod;
                 overridden is not null;
                 overridden = overridden.OverriddenMethod)
            {
                if (SymbolEqualityComparer.Default.Equals(overridden.OriginalDefinition, implementation))
                {
                    return candidate.OriginalDefinition;
                }
            }
        }

        return implementation;
    }

    /// <summary>
    /// Finds the operations from which an exception can escape a method body, following calls into
    /// methods, constructors, and local functions declared in the same compilation.
    /// </summary>
    private sealed class EscapeAnalysis
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _exception;

        public EscapeAnalysis(Compilation compilation, INamedTypeSymbol exception)
        {
            _compilation = compilation;
            _exception = exception;
        }

        public IEnumerable<IOperation> EscapingSites(IMethodSymbol method, CancellationToken cancellationToken)
        {
            var inProgress = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default) { method.OriginalDefinition };
            return Walk(method, inProgress, cancellationToken).Escapes.Select(e => e.Site).Distinct();
        }

        private Walker Walk(IMethodSymbol method, HashSet<IMethodSymbol> inProgress,
            CancellationToken cancellationToken)
        {
            var walker = new Walker(this, inProgress, cancellationToken);
            // A throw inside an async method is captured by the returned task and does not escape at the call site.
            if (method.IsAsync)
            {
                return walker;
            }

            var syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
            if (syntax is not null
                && _compilation.GetSemanticModel(syntax.SyntaxTree).GetOperation(syntax, cancellationToken) is { } body)
            {
                foreach (var child in body.ChildOperations)
                {
                    walker.Visit(child, null);
                }
            }

            return walker;
        }

        // Results are not cached across call sites: a cycle cut by the recursion guard would leave a callee's result
        // incomplete, and the hook methods this analysis starts from are few and small.
        private ImmutableArray<ITypeSymbol> EscapesFrom(IMethodSymbol callee, HashSet<IMethodSymbol> inProgress,
            CancellationToken cancellationToken)
        {
            callee = callee.OriginalDefinition;
            if (!inProgress.Add(callee))
            {
                return ImmutableArray<ITypeSymbol>.Empty;
            }

            var walker = Walk(callee, inProgress, cancellationToken);
            inProgress.Remove(callee);
            // An iterator method's body runs only when enumerated, so nothing escapes at the call site.
            return walker.IsIterator
                ? ImmutableArray<ITypeSymbol>.Empty
                : walker.Escapes.Select(e => e.Type).Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
                    .ToImmutableArray();
        }

        private sealed class Walker
        {
            private readonly EscapeAnalysis _analysis;
            private readonly HashSet<IMethodSymbol> _inProgress;
            private readonly CancellationToken _cancellationToken;

            public List<(IOperation Site, ITypeSymbol Type)> Escapes { get; } = new();
            public bool IsIterator { get; private set; }

            public Walker(EscapeAnalysis analysis, HashSet<IMethodSymbol> inProgress,
                CancellationToken cancellationToken)
            {
                _analysis = analysis;
                _inProgress = inProgress;
                _cancellationToken = cancellationToken;
            }

            /// <param name="caughtType">The type of the enclosing catch clause; gives a rethrow its static type.</param>
            public void Visit(IOperation operation, ITypeSymbol? caughtType)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                switch (operation)
                {
                    // Lambda bodies run when the delegate is invoked and local function bodies when they are called,
                    // so neither is part of the enclosing body; a local function is reached through its invocation.
                    case IAnonymousFunctionOperation:
                    case ILocalFunctionOperation:
                        return;
                    case ITryOperation tryOperation:
                        VisitTry(tryOperation, caughtType);
                        return;
                    case IReturnOperation { Kind: OperationKind.YieldReturn or OperationKind.YieldBreak }:
                        IsIterator = true;
                        break;
                    case IThrowOperation throwOperation:
                        Escapes.Add((operation, ThrownType(throwOperation) ?? caughtType ?? _analysis._exception));
                        break;
                    case IInvocationOperation invocation:
                        AddCallee(operation, invocation.TargetMethod);
                        break;
                    case IObjectCreationOperation { Constructor: { } constructor }:
                        AddCallee(operation, constructor);
                        break;
                }

                foreach (var child in operation.ChildOperations)
                {
                    Visit(child, caughtType);
                }
            }

            // The thrown expression is wrapped in an implicit conversion to System.Exception, whose type would make
            // every catch clause of a base type look like a mismatch.
            private static ITypeSymbol? ThrownType(IThrowOperation throwOperation)
            {
                var exception = throwOperation.Exception;
                while (exception is IConversionOperation { IsImplicit: true } conversion)
                {
                    exception = conversion.Operand;
                }

                return exception?.Type;
            }

            private void VisitTry(ITryOperation tryOperation, ITypeSymbol? caughtType)
            {
                var body = new Walker(_analysis, _inProgress, _cancellationToken);
                body.Visit(tryOperation.Body, caughtType);
                IsIterator |= body.IsIterator;
                Escapes.AddRange(body.Escapes.Where(e => !tryOperation.Catches.Any(c => Handles(c, e.Type))));

                foreach (var catchClause in tryOperation.Catches)
                {
                    Visit(catchClause.Handler, CaughtType(catchClause));
                }

                if (tryOperation.Finally is { } finallyBlock)
                {
                    Visit(finallyBlock, caughtType);
                }
            }

            private void AddCallee(IOperation site, IMethodSymbol callee)
            {
                foreach (var type in _analysis.EscapesFrom(callee, _inProgress, _cancellationToken))
                {
                    Escapes.Add((site, type));
                }
            }

            private bool Handles(ICatchClauseOperation catchClause, ITypeSymbol thrown)
            {
                // A filter can reject the exception, so a filtered clause never counts as handling it.
                if (catchClause.Filter is not null)
                {
                    return false;
                }

                var caught = CaughtType(catchClause);
                for (var type = thrown; type is not null; type = type.BaseType)
                {
                    if (SymbolEqualityComparer.Default.Equals(type, caught))
                    {
                        return true;
                    }
                }

                return false;
            }

            // A general catch clause has no exception type of its own (Roslyn reports System.Object); the caught type
            // is then System.Exception, which every C# exception derives from.
            private ITypeSymbol CaughtType(ICatchClauseOperation catchClause)
            {
                return catchClause.ExceptionType is { SpecialType: not SpecialType.System_Object } type
                    ? type
                    : _analysis._exception;
            }
        }
    }
}
