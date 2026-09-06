using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1005: Attributes implementing ICommandWrapper are not supported on async and coroutine-style test methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandWrapperOnAsyncTestAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1005";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Attributes implementing ICommandWrapper are not supported on async and coroutine-style test methods",
        messageFormat: "'{0}' is not supported on async and coroutine-style test methods.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method whose return type is System.Threading.Tasks.Task or System.Collections.IEnumerator and that is marked with an attribute implementing NUnit.Framework.Interfaces.ICommandWrapper. Unity Test Framework runs such methods through its own coroutine-driven commands, which a user-defined wrapper cannot drive; the test body never runs.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1005.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
