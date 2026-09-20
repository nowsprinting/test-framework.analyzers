using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1007: Only Task is supported as an async SetUp and TearDown method return type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonTaskAsyncSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1007";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only Task is supported as an async SetUp and TearDown method return type",
        messageFormat:
        "'{0}' is not supported as an async SetUp or TearDown method return type: the method is not awaited or the fixture fails. Return 'Task' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with SetUpAttribute or TearDownAttribute that has the async modifier or an awaitable return type, and whose return type is not System.Threading.Tasks.Task. Unity Test Framework awaits only Task-returning setup and teardown methods: an async void method is invoked but never awaited, and every other return type makes NUnit mark the whole fixture as not runnable.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1007.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
