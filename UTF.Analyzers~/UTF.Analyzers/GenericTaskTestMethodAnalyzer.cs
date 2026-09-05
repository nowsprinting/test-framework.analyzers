using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1002: Task&lt;TResult&gt; is not supported as a test method return type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GenericTaskTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Task<TResult> is not supported as a test method return type",
        messageFormat: "Task<TResult> is not supported as a test method return type. Return `Task` and assert the value inside the test method instead of using ExpectedResult.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Detects a test method whose return type is System.Threading.Tasks.Task<TResult>. Unity Test Framework runs only the non-generic Task as an async test; a Task<TResult> method with ExpectedResult is executed synchronously and freezes the Editor.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1002.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
