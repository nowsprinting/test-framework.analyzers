using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

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
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        var outerAction = compilation.GetTypeByMetadataName("UnityEngine.TestTools.IOuterUnityTestAction");
        var attributeUsage = compilation.GetTypeByMetadataName("System.AttributeUsageAttribute");
        var attribute = compilation.GetTypeByMetadataName("System.Attribute");
        if (outerAction is null || attributeUsage is null || attribute is null)
        {
            return;
        }

        // Two actions instead of one: a class that names the interface is reported at that base list entry, which a
        // symbol action cannot locate without Compilation.GetSemanticModel (RS1030); a class that inherits the interface
        // from its base class has no such entry and is reported at its name, which a syntax action on the base list never sees.
        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            var entry = (SimpleBaseTypeSyntax)nodeContext.Node;
            if (entry.Parent?.Parent is not ClassDeclarationSyntax classDeclaration
                || nodeContext.SemanticModel.GetTypeInfo(entry.Type, nodeContext.CancellationToken).Type is not
                    INamedTypeSymbol { TypeKind: TypeKind.Interface } named
                || !ActionAttributeAnalysis.IsOrDerivesFrom(named, outerAction)
                || nodeContext.SemanticModel.GetDeclaredSymbol(classDeclaration, nodeContext.CancellationToken) is not { } symbol
                || !ShouldReport(symbol, outerAction, attribute, attributeUsage))
            {
                return;
            }

            nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, entry.GetLocation()));
        }, SyntaxKind.SimpleBaseType);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var symbol = (INamedTypeSymbol)symbolContext.Symbol;
            if (symbol.TypeKind != TypeKind.Class || ActionAttributeAnalysis.NamesInterface(symbol, outerAction)
                || !ShouldReport(symbol, outerAction, attribute, attributeUsage))
            {
                return;
            }

            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0]));
        }, SymbolKind.NamedType);
    }

    private static bool ShouldReport(INamedTypeSymbol symbol, INamedTypeSymbol outerAction, INamedTypeSymbol attribute,
        INamedTypeSymbol attributeUsage)
    {
        return ActionAttributeAnalysis.DerivesFrom(symbol, attribute)
               && ActionAttributeAnalysis.Implements(symbol, outerAction)
               && (ActionAttributeAnalysis.EffectiveValidOn(symbol, attributeUsage) & ~AttributeTargets.Method) != 0;
    }
}
