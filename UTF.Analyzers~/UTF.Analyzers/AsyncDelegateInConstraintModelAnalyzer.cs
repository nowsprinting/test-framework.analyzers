using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF2003: Async delegates are not supported as the actual value of the constraint model.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncDelegateInConstraintModelAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2003";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Async delegates are not supported as the actual value of the constraint model",
        messageFormat:
        "Async delegates are not supported as the actual value of '{0}': the Editor freezes or the assertion fails. Await the operation in an async test method and assert on its result instead.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an async delegate passed as the actual value of Assert.That with a constraint that is not a Throws constraint, or passed as the actual value of Assume.That with any constraint. NUnit evaluates the delegate and waits for the returned Task synchronously on the calling thread. Unity Test Framework runs tests on the main thread, so the Editor freezes.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF2003.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var analysis = AsyncDelegateAnalysis.TryCreate(context.Compilation);
        if (analysis is null)
        {
            return;
        }

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            if (analysis.Classify(invocation, out var argument, out var receivingApi) ==
                AsyncDelegateAnalysis.Owner.ConstraintModel)
            {
                operationContext.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(), receivingApi));
            }
        }, OperationKind.Invocation);
    }
}
