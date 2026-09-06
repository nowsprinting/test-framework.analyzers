using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF2002: Async delegates are not supported as the actual value of Throws constraints, Assert.Throws, Assert.Catch, and Assert.DoesNotThrow.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncDelegateInThrowsConstraintAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Async delegates are not supported as the actual value of Throws constraints, Assert.Throws, Assert.Catch, and Assert.DoesNotThrow",
        messageFormat:
        "Async delegates are not supported as the actual value of '{0}'. Test the exception with try/catch in an async test method instead.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an async delegate passed as the actual value of a Throws constraint, or passed to Assert.Throws, Assert.Catch, or Assert.DoesNotThrow. NUnit waits for the returned Task synchronously on the calling thread. Unity Test Framework runs tests on the main thread, so the Editor freezes.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2002.md");

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
        var throws = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
        var resolveConstraint =
            context.Compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
        if (assert is null || throws is null || resolveConstraint is null)
        {
            return;
        }

        var throwsConstraintTypes = AsyncDelegateAnalysis.ThrowsConstraintTypes(context.Compilation);
        var that = assert.GetMembers("That").OfType<IMethodSymbol>()
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);
        var testDelegateAssertions = new[] { "Throws", "Catch", "DoesNotThrow" }
            .SelectMany(name => assert.GetMembers(name).OfType<IMethodSymbol>())
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            var method = invocation.TargetMethod.OriginalDefinition;
            var isThat = that.Contains(method);
            if (!isThat && !testDelegateAssertions.Contains(method))
            {
                return;
            }

            // The delegate is checked before the constraint because it is the cheaper and more selective filter:
            // most Assert.That calls take a plain value, and most Assert.Throws calls take a synchronous lambda.
            var argument = AsyncDelegateAnalysis.DelegateArgument(invocation);
            if (argument is null || !AsyncDelegateAnalysis.IsAsyncDelegate(argument))
            {
                return;
            }

            // The name is built from the symbol rather than the syntax so that a call through "using static" still reads "Throws.X".
            var receivingApi = $"{assert.Name}.{method.Name}";
            if (isThat)
            {
                var root = AsyncDelegateAnalysis.ThrowsRoot(
                    AsyncDelegateAnalysis.ConstraintArgument(invocation, resolveConstraint), throws,
                    throwsConstraintTypes);
                if (root is null)
                {
                    return;
                }

                receivingApi = root;
            }

            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(), receivingApi));
        }, OperationKind.Invocation);
    }
}
