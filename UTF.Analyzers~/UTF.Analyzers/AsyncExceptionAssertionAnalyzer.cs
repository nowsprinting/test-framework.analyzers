using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers;

/// <summary>
/// UTF2001: Assert.ThrowsAsync, CatchAsync, and DoesNotThrowAsync are not supported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncExceptionAssertionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Assert.ThrowsAsync, CatchAsync, and DoesNotThrowAsync are not supported",
        messageFormat:
        "'{0}' is not supported. Test the exception with try/catch in an 'async Task' test method instead.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects any call to Assert.ThrowsAsync, Assert.CatchAsync, or Assert.DoesNotThrowAsync. These methods wait for the returned Task synchronously on the calling thread. Unity Test Framework runs tests on the main thread, so the Editor freezes.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2001.md");

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
        if (assert is null)
        {
            return;
        }

        // Every overload (generic, Type, IResolveConstraint) is collected once from the Assert type so that the
        // per-invocation check is a symbol comparison; matching ContainingType plus TargetMethod.Name per invocation
        // would work too, but the repository convention is to compare symbols, never name strings.
        var reported = new[] { "ThrowsAsync", "CatchAsync", "DoesNotThrowAsync" }
            .SelectMany(name => assert.GetMembers(name).OfType<IMethodSymbol>())
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var method = ((IInvocationOperation)operationContext.Operation).TargetMethod.OriginalDefinition;
            if (!reported.Contains(method))
            {
                return;
            }

            // The name is built from the symbol rather than the syntax so that a call through "using static" still reads "Assert.X".
            var location = operationContext.Operation.Syntax.GetLocation();
            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, location,
                $"{method.ContainingType.Name}.{method.Name}"));
        }, OperationKind.Invocation);
    }
}
