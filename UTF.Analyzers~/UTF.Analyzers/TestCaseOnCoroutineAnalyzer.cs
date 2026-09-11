using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF1001: TestCase and TestCaseSource attributes are not supported on coroutine-style test methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestCaseOnCoroutineAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "TestCase and TestCaseSource attributes are not supported on coroutine-style test methods",
        messageFormat: "'{0}' is not supported on coroutine-style test methods: the test never runs. Use an 'async Task' test method instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Detects TestCaseAttribute or TestCaseSourceAttribute applied to a test method whose return type is System.Collections.IEnumerator (a coroutine-style test method).",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1001.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(c => CoroutineAttributeAnalysis.Register(c, Rule,
            "NUnit.Framework.TestCaseAttribute",
            "NUnit.Framework.TestCaseSourceAttribute"));
    }
}
