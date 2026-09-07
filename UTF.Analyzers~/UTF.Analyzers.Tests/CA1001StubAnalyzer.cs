using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Tests
{
    /// <summary>
    /// Reports CA1001 at every class declaring an IDisposable-typed instance field so that a suppressor test has something to suppress.
    /// Referencing the real Microsoft.CodeAnalysis.NetAnalyzers would run every CA rule on the dummies and fixtures.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CA1001StubAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new(
            DisposableFieldDisposedInUnityTearDownSuppressor.SuppressedDiagnosticId,
            title: "Types that own disposable fields should be disposable",
            messageFormat: "Types that own disposable fields should be disposable",
            category: "Design",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(symbolContext =>
            {
                var type = (INamedTypeSymbol)symbolContext.Symbol;
                var disposable = symbolContext.Compilation.GetSpecialType(SpecialType.System_IDisposable);
                if (type.TypeKind == TypeKind.Class &&
                    type.GetMembers().OfType<IFieldSymbol>().Any(field =>
                        !field.IsStatic &&
                        field.Type.AllInterfaces.Contains(disposable, SymbolEqualityComparer.Default)))
                {
                    // NetAnalyzers reports at the type symbol's location, i.e. the identifier.
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0]));
                }
            }, SymbolKind.NamedType);
        }
    }
}
