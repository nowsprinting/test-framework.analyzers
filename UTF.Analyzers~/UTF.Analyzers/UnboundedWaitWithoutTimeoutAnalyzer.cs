using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4001: Test methods that wait for a condition must have a Timeout attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnboundedWaitWithoutTimeoutAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Test methods that wait for a condition must have a Timeout attribute",
        messageFormat:
        "'{0}' waits for a condition without a time limit: the test runs for 180 seconds when the condition never holds. Apply a Timeout attribute with a short value to the test method.",
        category: "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method that waits for a condition with no time limit of its own, when neither the method, its containing type, nor the assembly has a Timeout attribute. A while or do loop that yields or awaits in its body and is not a deadline loop that reads a clock, and a call to WaitUntil, WaitWhile, or the UniTask.WaitUntil family that is not bounded by a timeout, are recognized, directly in the test method or in helpers, lambdas, and local functions up to two levels deep. When the condition never holds, such a test runs until the 180-second default timeout of Unity Test Framework.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4001.md");

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
        var timeout = compilation.GetTypeByMetadataName("NUnit.Framework.TimeoutAttribute");
        if (timeout is null || HasTimeout(compilation.Assembly, timeout))
        {
            return;
        }

        var testMethods = TestMethodAnalysis.TryCreate(compilation);
        if (testMethods is null)
        {
            return;
        }

        var analysis = new WaitAnalysis(compilation);
        context.RegisterSymbolAction(symbolContext =>
        {
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!testMethods.IsTestMethod(method)
                || HasTimeout(method, timeout)
                || HasTimeout(method.ContainingType, timeout))
            {
                return;
            }

            foreach (var (location, name) in analysis.Waits(method, symbolContext.CancellationToken))
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, name));
            }
        }, SymbolKind.Method);
    }

    // Only the symbol's own attributes are read: TimeoutAttribute is declared Inherited = false, so a Timeout on a
    // base class does not reach the fixture at run time.
    private static bool HasTimeout(ISymbol symbol, INamedTypeSymbol timeout)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass?.OriginalDefinition, timeout))
            {
                return true;
            }
        }

        return false;
    }
}
