using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1006: Only Task is supported as an async test method return type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonTaskAsyncTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1006";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only Task is supported as an async test method return type",
        messageFormat:
        "'{0}' is not supported as an async test method return type: the test fails without running. Return 'Task' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method that has the async modifier or an awaitable return type, and whose return type is not System.Threading.Tasks.Task. Unity Test Framework runs only the non-generic Task as an async test; every other return type, such as UniTask, UniTaskVoid, Awaitable, or ValueTask, is run through NUnit's synchronous command and fails before the test body completes.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1006.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
