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

        // A syntax node action on the class declaration rather than a symbol action: the location is the base list entry
        // that names the interface, and locating it from a symbol action needs Compilation.GetSemanticModel (RS1030).
        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            var classDeclaration = (ClassDeclarationSyntax)nodeContext.Node;
            var symbol = nodeContext.SemanticModel.GetDeclaredSymbol(classDeclaration, nodeContext.CancellationToken);
            if (symbol is null || !DerivesFrom(symbol, attribute) || !Implements(symbol, outerAction)
                || (EffectiveValidOn(symbol, attribute, attributeUsage) & ~AttributeTargets.Method) == 0)
            {
                return;
            }

            var entry = FindInterfaceEntry(classDeclaration, outerAction, nodeContext);
            if (entry is not null)
            {
                nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, entry.GetLocation()));
                return;
            }

            // Another declaration of the partial class names the interface and reports it; otherwise the interface comes
            // from a base class and the first declaration reports at the class name.
            if (NamesInterface(symbol, outerAction) || symbol.DeclaringSyntaxReferences[0].GetSyntax(nodeContext.CancellationToken) != classDeclaration)
            {
                return;
            }

            nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation()));
        }, SyntaxKind.ClassDeclaration);
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

    private static bool Implements(INamedTypeSymbol symbol, INamedTypeSymbol outerAction)
    {
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
            if (SymbolEqualityComparer.Default.Equals(declared, outerAction) || Implements(declared, outerAction))
            {
                return true;
            }
        }

        return false;
    }

    private static BaseTypeSyntax? FindInterfaceEntry(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol outerAction,
        SyntaxNodeAnalysisContext context)
    {
        if (classDeclaration.BaseList is null)
        {
            return null;
        }

        foreach (var entry in classDeclaration.BaseList.Types)
        {
            if (context.SemanticModel.GetTypeInfo(entry.Type, context.CancellationToken).Type is INamedTypeSymbol
                {
                    TypeKind: TypeKind.Interface
                } named
                && (SymbolEqualityComparer.Default.Equals(named, outerAction) || Implements(named, outerAction)))
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>
    /// The AttributeUsage the compiler applies: the class's own, else the nearest base class's, else All.
    /// Roslyn's GetAttributeUsageInfo is internal, so the base chain is walked here.
    /// </summary>
    private static AttributeTargets EffectiveValidOn(INamedTypeSymbol symbol, INamedTypeSymbol attribute,
        INamedTypeSymbol attributeUsage)
    {
        for (var type = symbol; type is not null && !SymbolEqualityComparer.Default.Equals(type, attribute); type = type.BaseType)
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
