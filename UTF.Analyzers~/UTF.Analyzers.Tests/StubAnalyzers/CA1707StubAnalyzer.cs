using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Tests.StubAnalyzers
{
    /// <summary>
    /// Reports CA1707 at the identifier of every ordinary method whose name contains an underscore so that a suppressor test
    /// has something to suppress. Referencing the real Microsoft.CodeAnalysis.NetAnalyzers would run every CA rule on the
    /// dummies and fixtures.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CA1707StubAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new(
            UnderscoreInTestMethodNameSuppressor.SuppressedDiagnosticId,
            title: "Identifiers should not contain underscores",
            messageFormat: "Remove the underscores from member name {0}",
            category: "Naming",
            // Same default severity as the real CA1707 (RuleLevel.IdeHidden_BulkConfigurable in Microsoft.CodeAnalysis.NetAnalyzers);
            // Roslyn never passes an Error-by-default diagnostic to a suppressor, so the stub must not differ from the real rule here.
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;
                if (method.MethodKind == MethodKind.Ordinary && method.Name.Contains("_"))
                {
                    // Microsoft.CodeAnalysis.NetAnalyzers reports at the symbol's location, i.e. the identifier.
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
                }
            }, SymbolKind.Method);
        }
    }
}
