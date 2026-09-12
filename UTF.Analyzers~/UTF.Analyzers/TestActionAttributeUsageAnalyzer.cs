using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF5004: Attributes implementing ITestAction must restrict their targets with AttributeUsage.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestActionAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5004";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Attributes implementing ITestAction must restrict their targets with AttributeUsage",
        messageFormat: "Attributes implementing ITestAction whose Targets is {0} are read only from {1}; applied to other targets, the action is silently skipped. Restrict the targets with AttributeUsage.",
        category: "Extensions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects an attribute class that implements NUnit.Framework.ITestAction and whose effective AttributeUsage allows a target from which Unity Test Framework does not run the action for the attribute's Targets value. The supported targets are AttributeTargets.Method when Targets is ActionTargets.Test, and AttributeTargets.Class, AttributeTargets.Interface, and AttributeTargets.Assembly when Targets is ActionTargets.Suite or ActionTargets.Default. Without the restriction, the compiler accepts the attribute on an unsupported target, and the action is silently skipped there.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5004.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
