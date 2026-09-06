using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using UTF.Analyzers.Utilities;

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
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF2005.md");

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

        // After is non-virtual on Constraint, so a call on any subclass resolves to one of these symbols.
        var afterMethods = AsyncDelegateAnalysis.Methods(constraint, "After");

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var location = operationContext.Operation switch
            {
                IInvocationOperation invocation when afterMethods.Contains(invocation.TargetMethod.OriginalDefinition) =>
                    AfterLocation(invocation),
                IObjectCreationOperation creation when SymbolEqualityComparer.Default.Equals(
                    creation.Type?.OriginalDefinition, delayedConstraint) => creation.Syntax.GetLocation(),
                _ => null
            };
            if (location is not null)
            {
                operationContext.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }, OperationKind.Invocation, OperationKind.ObjectCreation);
    }

    /// <summary>
    /// Highlights only "After(...)" rather than the whole chain, which usually starts with an unrelated
    /// constraint such as Is.EqualTo(...) that the user must keep.
    /// </summary>
    private static Location AfterLocation(IInvocationOperation invocation)
    {
        var syntax = invocation.Syntax;
        var start = syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access }
            ? access.Name.SpanStart
            : syntax.SpanStart;
        return Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(start, syntax.Span.End));
    }
}
