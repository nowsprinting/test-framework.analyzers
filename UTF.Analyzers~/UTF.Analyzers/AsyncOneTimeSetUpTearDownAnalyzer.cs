using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1004: OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncOneTimeSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1004";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods",
        messageFormat:
        "'{0}' is not supported on methods that return Task or have the async modifier. Use '{1}' with a coroutine-style test method instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with OneTimeSetUpAttribute or OneTimeTearDownAttribute whose return type is System.Threading.Tasks.Task (or Task<TResult>), or that has the async modifier. Unity Test Framework runs these methods through NUnit's synchronous command, so a Task-returning method freezes the Editor and an async void method fails.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1004.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
