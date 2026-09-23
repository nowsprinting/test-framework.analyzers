using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF1009: Only IEnumerator is supported as the return type of methods with the UnitySetUp, UnityTearDown,
/// UnityOneTimeSetUp, and UnityOneTimeTearDown attributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonEnumeratorUnitySetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1009";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Only IEnumerator is supported as the return type of methods with the UnitySetUp, UnityTearDown, UnityOneTimeSetUp, and UnityOneTimeTearDown attributes",
        messageFormat:
        "'{0}' is not supported as the return type of a method marked with '{1}': the method never runs. Return 'IEnumerator' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with UnitySetUpAttribute, UnityTearDownAttribute, UnityOneTimeSetUpAttribute, or UnityOneTimeTearDownAttribute whose return type is not System.Collections.IEnumerator. Unity Test Framework does not recognize such a method as a setup or teardown method, so it never runs, and the tests run without it and report no error.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1009.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // UnityOneTimeSetUp/UnityOneTimeTearDown exist only in Unity Test Framework 1.5.0 or later; TryCreate skips
        // unresolved names so that UnitySetUp/UnityTearDown are still inspected with earlier versions.
        var hookAttributes = MethodAttributeAnalysis.TryCreate(context.Compilation,
            MethodAttributeAnalysis.UnitySetUpTearDownAttributes);
        var enumerator = context.Compilation.GetTypeByMetadataName("System.Collections.IEnumerator");
        if (hookAttributes is null || enumerator is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            // Equality rather than assignability: Unity Test Framework collects these methods with
            // type == method.ReturnType, so an IEnumerator<T> that implements IEnumerator is still dropped.
            if (SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            {
                return;
            }

            var attribute = hookAttributes.FindAttribute(method);
            if (attribute is null)
            {
                return;
            }

            var location = MethodAttributeAnalysis.ReturnTypeLocation(method, symbolContext.CancellationToken);
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location,
                method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), attribute.Name));
        }, SymbolKind.Method);
    }
}
