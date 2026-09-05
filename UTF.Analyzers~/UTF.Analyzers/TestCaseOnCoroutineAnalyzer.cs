using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1001: TestCase and TestCaseSource are not supported on coroutine test methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestCaseOnCoroutineAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "TestCase and TestCaseSource are not supported on coroutine test methods",
        messageFormat: "Method-level parameterized tests cannot be used on coroutine-style test methods. Use the `async` keyword instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Detects TestCaseAttribute or TestCaseSourceAttribute placed on a test method whose return type is System.Collections.IEnumerator (a coroutine-style test method).",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/rules/UTF1001.html");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var testCase = context.Compilation.GetTypeByMetadataName("NUnit.Framework.TestCaseAttribute");
        var testCaseSource = context.Compilation.GetTypeByMetadataName("NUnit.Framework.TestCaseSourceAttribute");
        if (testCase is null && testCaseSource is null)
        {
            return;
        }

        // GetSpecialType is used instead of GetTypeByMetadataName: it never returns null, and matching the exact
        // non-generic IEnumerator symbol excludes IEnumerator<T> without extra checks.
        var enumerator = context.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            {
                return;
            }

            if (method.GetAttributes().Any(a => IsOneOf(a.AttributeClass, testCase, testCaseSource)))
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0]));
            }
        }, SymbolKind.Method);
    }

    private static bool IsOneOf(INamedTypeSymbol? attribute, INamedTypeSymbol? a, INamedTypeSymbol? b)
    {
        var original = attribute?.OriginalDefinition;
        return original is not null &&
               (SymbolEqualityComparer.Default.Equals(original, a) || SymbolEqualityComparer.Default.Equals(original, b));
    }
}
