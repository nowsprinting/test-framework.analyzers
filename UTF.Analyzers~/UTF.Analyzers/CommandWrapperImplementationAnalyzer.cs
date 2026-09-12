using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF5002: Implementing IWrapTestMethod and IWrapSetUpTearDown is not recommended.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandWrapperImplementationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Implementing IWrapTestMethod and IWrapSetUpTearDown is not recommended",
        messageFormat: "Attributes implementing '{0}' cannot be applied to async and coroutine-style test methods. Implement IOuterUnityTestAction instead.",
        category: "Extensions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a class whose base list names NUnit.Framework.Interfaces.IWrapTestMethod or NUnit.Framework.Interfaces.IWrapSetUpTearDown. Unity Test Framework cannot run the command returned by a user-defined Wrap on async Task and coroutine-style test methods, so such an attribute can be applied only to synchronous test methods. UnityEngine.TestTools.IOuterUnityTestAction provides before/after hooks that work on every kind of test method.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5002.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
