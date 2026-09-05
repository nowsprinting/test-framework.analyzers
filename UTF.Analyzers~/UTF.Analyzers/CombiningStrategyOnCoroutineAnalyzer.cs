using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1003: Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CombiningStrategyOnCoroutineAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1003";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods",
        messageFormat: "'{0}' cannot be used on coroutine-style test methods. Use an 'async Task' test method instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Detects PairwiseAttribute, SequentialAttribute, or CombinatorialAttribute applied to a test method whose return type is System.Collections.IEnumerator (a coroutine-style test method).",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1003.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // The three concrete attributes are matched rather than their CombiningStrategyAttribute base class,
        // so user-defined strategies stay out of scope as the specification states.
        var targets = new[]
            {
                context.Compilation.GetTypeByMetadataName("NUnit.Framework.PairwiseAttribute"),
                context.Compilation.GetTypeByMetadataName("NUnit.Framework.SequentialAttribute"),
                context.Compilation.GetTypeByMetadataName("NUnit.Framework.CombinatorialAttribute"),
            }
            .Where(t => t is not null)
            .ToImmutableArray();
        if (targets.IsEmpty)
        {
            return;
        }

        // GetSpecialType is used instead of GetTypeByMetadataName: it never returns null, and matching the exact
        // non-generic IEnumerator symbol excludes IEnumerator<T> without extra checks.
        var enumerator = context.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            {
                return;
            }

            foreach (var attribute in method.GetAttributes())
            {
                if (!targets.Contains(attribute.AttributeClass?.OriginalDefinition, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                // Reported at the attribute rather than the method name so that each offending attribute is highlighted.
                // ApplicationSyntaxReference is null only for attributes from metadata, which a SymbolAction on source methods never sees.
                var location = attribute.ApplicationSyntaxReference?.GetSyntax(symbolContext.CancellationToken).GetLocation()
                               ?? method.Locations[0];
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, attribute.AttributeClass!.Name));
            }
        }, SymbolKind.Method);
    }
}
