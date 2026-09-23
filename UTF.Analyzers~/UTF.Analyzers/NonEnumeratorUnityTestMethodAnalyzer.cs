using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF1008: Only IEnumerator is supported as the return type of methods with the UnityTest attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonEnumeratorUnityTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1008";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only IEnumerator is supported as the return type of methods with the UnityTest attribute",
        messageFormat:
        "'{0}' is not supported as the return type of a UnityTest method: the test never runs. Return 'IEnumerator' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with UnityTestAttribute whose return type is not System.Collections.IEnumerator. Unity Test Framework marks such a test as not runnable, so it fails without running.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1008.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var unityTestAttribute = MethodAttributeAnalysis.TryCreate(context.Compilation,
            ImmutableArray.Create("UnityEngine.TestTools.UnityTestAttribute"));
        var enumerator = context.Compilation.GetTypeByMetadataName("System.Collections.IEnumerator");
        if (unityTestAttribute is null || enumerator is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            // Equality rather than assignability: UnityTestAttribute compares with Type == typeof(IEnumerator), so an
            // IEnumerator<T> that implements IEnumerator is still rejected at run time.
            if (SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            {
                return;
            }

            if (!unityTestAttribute.HasAttribute(method))
            {
                return;
            }

            var location = MethodAttributeAnalysis.ReturnTypeLocation(method, symbolContext.CancellationToken);
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location,
                method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }, SymbolKind.Method);
    }
}
