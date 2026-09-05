using System.Collections.Immutable;
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
    }
}
