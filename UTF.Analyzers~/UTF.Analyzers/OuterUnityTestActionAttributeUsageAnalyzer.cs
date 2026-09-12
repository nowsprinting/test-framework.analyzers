using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                || !IsOrDerivesFrom(named, outerAction)
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
            if (symbol.TypeKind != TypeKind.Class || NamesInterface(symbol, outerAction)
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
        return DerivesFrom(symbol, attribute)
               && Implements(symbol, outerAction)
               && (EffectiveValidOn(symbol, attributeUsage) & ~AttributeTargets.Method) != 0;
    }

    private static bool DerivesFrom(INamedTypeSymbol symbol, INamedTypeSymbol attribute)
    {
        for (var type = symbol.BaseType; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type, attribute))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsOrDerivesFrom(INamedTypeSymbol candidate, INamedTypeSymbol outerAction)
    {
        return SymbolEqualityComparer.Default.Equals(candidate, outerAction) || Implements(candidate, outerAction);
    }

    private static bool Implements(INamedTypeSymbol symbol, INamedTypeSymbol outerAction)
    {
        // foreach instead of the LINQ Contains overload: every named type and base list entry in the compilation reaches
        // here, and the LINQ overload boxes the ImmutableArray on each call.
        foreach (var inherited in symbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(inherited, outerAction))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether the class's own base list (any declaration) names the interface or an interface derived from it.
    /// </summary>
    private static bool NamesInterface(INamedTypeSymbol symbol, INamedTypeSymbol outerAction)
    {
        foreach (var declared in symbol.Interfaces)
        {
            if (IsOrDerivesFrom(declared, outerAction))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The AttributeUsage the compiler applies: the class's own, else the nearest base class's, else All.
    /// Roslyn's GetAttributeUsageInfo is internal, so the base chain is walked here.
    /// </summary>
    private static AttributeTargets EffectiveValidOn(INamedTypeSymbol symbol, INamedTypeSymbol attributeUsage)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            foreach (var data in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attributeUsage)
                    && data.ConstructorArguments.Length == 1
                    && data.ConstructorArguments[0].Value is int validOn)
                {
                    return (AttributeTargets)validOn;
                }
            }
        }

        return AttributeTargets.All;
    }
}
