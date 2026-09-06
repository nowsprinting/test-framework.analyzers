using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers;

/// <summary>
/// UTF2005: DelayedConstraint is not supported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DelayedConstraintAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2005";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "DelayedConstraint is not supported",
        messageFormat:
        "DelayedConstraint is not supported. Wait for the condition in a coroutine-style or 'async Task' test method and assert afterwards.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects any call to Constraint.After and any new DelayedConstraint(...). DelayedConstraint waits with Thread.Sleep on the calling thread. Unity Test Framework runs tests on the main thread, so nothing that is driven by the main thread can change while it waits, and a condition driven by the main thread never becomes true.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2005.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var constraint = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.Constraint");
        var delayedConstraint = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.DelayedConstraint");
        if (constraint is null || delayedConstraint is null)
        {
            return;
        }

        // After is non-virtual on Constraint, so a call on any subclass resolves to one of these symbols; the
        // overloads are collected once rather than matched by name per invocation, following the repository convention.
        var afterMethods = constraint.GetMembers("After").OfType<IMethodSymbol>()
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var operation = operationContext.Operation;
            var reported = operation switch
            {
                IInvocationOperation invocation => afterMethods.Contains(invocation.TargetMethod.OriginalDefinition),
                IObjectCreationOperation creation => SymbolEqualityComparer.Default.Equals(creation.Type?.OriginalDefinition, delayedConstraint),
                _ => false
            };
            if (reported)
            {
                operationContext.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation()));
            }
        }, OperationKind.Invocation, OperationKind.ObjectCreation);
    }
}
