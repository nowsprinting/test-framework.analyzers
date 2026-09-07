using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Tests.StubAnalyzers
{
    /// <summary>
    /// Reports NUnit1028 at every public ordinary method that is not a test, setup, or teardown method of NUnit, in a class
    /// that declares at least one such method, so that a suppressor test has something to suppress. The real NUnit.Analyzers
    /// cannot be used here: the dummies are compiled into the test assembly, not into nunit.framework. The fixture gate of the
    /// real rule is reproduced because the dummies themselves declare public methods.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NUnit1028StubAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new(
            UnityHookMethodIsPublicSuppressor.SuppressedDiagnosticId,
            title: "The non-test method is public",
            messageFormat: "Only test methods should be public",
            category: "Structure",
            // Same default severity as the real NUnit1028 (NUnit.Analyzers 3.9.0);
            // Roslyn never passes an Error-by-default diagnostic to a suppressor, so the stub must not differ from the real rule here.
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        private static readonly ImmutableArray<string> TestRelatedAttributes = ImmutableArray.Create(
            "NUnit.Framework.TestAttribute",
            "NUnit.Framework.SetUpAttribute",
            "NUnit.Framework.TearDownAttribute",
            "NUnit.Framework.OneTimeSetUpAttribute",
            "NUnit.Framework.OneTimeTearDownAttribute",
            "UnityEngine.TestTools.UnityTestAttribute");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(symbolContext =>
            {
                var methods = ((INamedTypeSymbol)symbolContext.Symbol).GetMembers().OfType<IMethodSymbol>()
                    .Where(method => method.MethodKind == MethodKind.Ordinary).ToList();
                if (!methods.Any(IsTestRelated))
                {
                    return;
                }

                foreach (var method in methods.Where(method =>
                             method.DeclaredAccessibility == Accessibility.Public && !IsTestRelated(method)))
                {
                    // NUnit.Analyzers reports at the method symbol's location, i.e. the identifier.
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0]));
                }
            }, SymbolKind.NamedType);
        }

        private static bool IsTestRelated(IMethodSymbol method)
        {
            return method.GetAttributes().Any(attribute =>
                attribute.AttributeClass is { } attributeClass &&
                TestRelatedAttributes.Contains(attributeClass.ToDisplayString()));
        }
    }
}
