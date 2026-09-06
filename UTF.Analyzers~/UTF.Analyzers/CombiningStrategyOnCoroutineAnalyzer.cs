using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF1003: Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CombiningStrategyOnCoroutineAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1003";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods",
        messageFormat: "'{0}' cannot be used on coroutine-style test methods. Use an 'async Task' test method instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Detects PairwiseAttribute, SequentialAttribute, or CombinatorialAttribute applied to a test method whose return type is System.Collections.IEnumerator (a coroutine-style test method).",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1003.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        // The three concrete attributes are matched rather than their CombiningStrategyAttribute base class,
        // so user-defined strategies stay out of scope as the specification states.
        context.RegisterCompilationStartAction(c => CoroutineAttributeAnalysis.Register(c, Rule,
            "NUnit.Framework.PairwiseAttribute",
            "NUnit.Framework.SequentialAttribute",
            "NUnit.Framework.CombinatorialAttribute"));
    }
}
