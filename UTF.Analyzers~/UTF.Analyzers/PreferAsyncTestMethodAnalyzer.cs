using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4006: Async Task test methods are recommended over coroutine-style test methods with the UnityTest attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferAsyncTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4006";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Async Task test methods are recommended over coroutine-style test methods with the UnityTest attribute",
        messageFormat:
        "'{0}' is a coroutine-style test method that yields only Unity yield instructions: it cannot use TestCase attributes or catch an exception across a wait. Use an 'async Task' test method instead.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a coroutine-style test method whose every yield return yields null or a Unity yield instruction, so that the method can be written as an async Task test method with no other change. A coroutine-style test method that yields anything else, e.g. an IEnumerator returned by the code under test, is not reported.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4006.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var testMethods = TestMethodAnalysis.TryCreate(context.Compilation);
        var walk = UnityYieldWalk.TryCreate(context.Compilation);
        if (testMethods is null || walk is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (testMethods.IsTestMethod(method)
                && walk.IsConvertibleCoroutine(method, symbolContext.CancellationToken))
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule,
                    UnityYieldWalk.SignatureLocation(method, symbolContext.CancellationToken), method.Name));
            }
        }, SymbolKind.Method);
    }
}
