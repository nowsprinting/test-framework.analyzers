using System.Collections.Immutable;
using System.Linq;
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
        "Async delegates are not supported as the actual value of '{0}'. Await the operation in an async test method and assert on its result instead.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an async delegate passed as the actual value of Assert.That with a constraint that is not a Throws constraint, or passed as the actual value of Assume.That with any constraint. NUnit evaluates the delegate and waits for the returned Task synchronously on the calling thread. Unity Test Framework runs tests on the main thread, so the Editor freezes.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2003.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var assert = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        var assume = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Assume");
        var throws = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
        var resolveConstraint =
            context.Compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
        if (assert is null || throws is null || resolveConstraint is null)
        {
            return;
        }

        var throwsConstraintTypes = AsyncDelegateAnalysis.ThrowsConstraintTypes(context.Compilation);
        var assertThat = assert.GetMembers("That").OfType<IMethodSymbol>()
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);
        // Assume is looked up separately from Assert so that a compilation without Assume still analyzes Assert.That.
        var assumeThat = (assume?.GetMembers("That").OfType<IMethodSymbol>() ?? Enumerable.Empty<IMethodSymbol>())
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            var method = invocation.TargetMethod.OriginalDefinition;
            var isAssert = assertThat.Contains(method);
            if (!isAssert && !assumeThat.Contains(method))
            {
                return;
            }

            var argument = AsyncDelegateAnalysis.DelegateArgument(invocation);
            if (argument is null || !AsyncDelegateAnalysis.IsAsyncDelegate(argument))
            {
                return;
            }

            // Only Assert.That defers to UTF2002; Assume.That is reported with every constraint, Throws included.
            if (isAssert && AsyncDelegateAnalysis.ThrowsRoot(
                    AsyncDelegateAnalysis.ConstraintArgument(invocation, resolveConstraint), throws,
                    throwsConstraintTypes) is not null)
            {
                return;
            }

            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(),
                $"{method.ContainingType.Name}.{method.Name}"));
        }, OperationKind.Invocation);
    }
}
