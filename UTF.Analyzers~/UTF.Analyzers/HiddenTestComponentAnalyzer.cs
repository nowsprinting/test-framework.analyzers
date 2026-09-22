using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF4005: MonoBehaviour classes in test assemblies should be hidden from the Add Component menu.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HiddenTestComponentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4005";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "MonoBehaviour classes in test assemblies should be hidden from the Add Component menu",
        messageFormat: "'{0}' is declared in a test assembly and appears in the Add Component menu of the Unity Editor, where it can be attached to a scene object by mistake. Apply AddComponentMenu with a menu name that starts with '/' to hide it.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a class deriving from UnityEngine.MonoBehaviour that is declared in a test assembly (assembly name ending with .Tests, or a source file under a Tests directory) and is not hidden from the Unity Editor's Add Component menu with [AddComponentMenu(\"/\")]. Test doubles that derive from MonoBehaviour exist only to be attached from test code, but the Editor lists every MonoBehaviour in the picker, so they show up next to the production components.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4005.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
