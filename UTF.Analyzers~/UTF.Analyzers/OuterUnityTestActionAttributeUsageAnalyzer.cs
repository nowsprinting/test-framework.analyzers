using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF5003: Attributes implementing IOuterUnityTestAction must restrict their targets with AttributeUsage.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OuterUnityTestActionAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5003";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Attributes implementing IOuterUnityTestAction must restrict their targets with AttributeUsage",
        messageFormat: "Attributes implementing IOuterUnityTestAction are read only from test methods; applied to other targets, the action is silently skipped. Restrict the targets to AttributeTargets.Method with AttributeUsage.",
        category: "Extensions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects an attribute class that implements UnityEngine.TestTools.IOuterUnityTestAction and whose effective AttributeUsage allows a target other than AttributeTargets.Method. Unity Test Framework reads IOuterUnityTestAction from the test method only. Without the restriction, the compiler accepts the attribute on a fixture class or an assembly, and the action is silently skipped there.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5003.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
