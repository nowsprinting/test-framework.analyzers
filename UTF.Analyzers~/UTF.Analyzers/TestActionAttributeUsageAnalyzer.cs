using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF5004: Attributes implementing ITestAction must restrict their targets with AttributeUsage.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestActionAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5004";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Attributes implementing ITestAction must restrict their targets with AttributeUsage",
        messageFormat: "Attributes implementing ITestAction whose Targets is {0} are read only from {1}; applied to other targets, the action is silently skipped. Restrict the targets with AttributeUsage.",
        category: "Extensions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects an attribute class that implements NUnit.Framework.ITestAction and whose effective AttributeUsage allows a target from which Unity Test Framework does not run the action for the attribute's Targets value. The supported targets are AttributeTargets.Method when Targets is ActionTargets.Test, and AttributeTargets.Class, AttributeTargets.Interface, and AttributeTargets.Assembly when Targets is ActionTargets.Suite or ActionTargets.Default. Without the restriction, the compiler accepts the attribute on an unsupported target, and the action is silently skipped there.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5004.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private const int ActionTargetsDefault = 0;
    private const int ActionTargetsTest = 1;
    private const int ActionTargetsSuite = 2;
    private const AttributeTargets SuiteTargets = AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly;

    /// <summary>
    /// Sentinel for a Targets getter whose returned value is not a single ActionTargets constant.
    /// </summary>
    private const int NotAConstant = -1;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        var testAction = compilation.GetTypeByMetadataName("NUnit.Framework.ITestAction");
        var attributeUsage = compilation.GetTypeByMetadataName("System.AttributeUsageAttribute");
        var attribute = compilation.GetTypeByMetadataName("System.Attribute");
        if (testAction is null || attributeUsage is null || attribute is null)
        {
            return;
        }

        var targetsProperty = testAction.GetMembers("Targets").OfType<IPropertySymbol>().FirstOrDefault();
        if (targetsProperty is null)
        {
            return;
        }

        var testActionAttribute = compilation.GetTypeByMetadataName("NUnit.Framework.TestActionAttribute");

        // Two actions instead of one, as in UTF5003: the base list entry is reported from the syntax action and a class
        // that inherits the interface from its name in the symbol action. Symbol scope rather than compilation end: the
        // Targets getter the class inherits may sit in another file, but its syntax is reachable through the symbol without
        // a SemanticModel, so nothing has to wait for another file's analysis, and the IDE shows the diagnostic while editing.
        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            if (ActionAttributeAnalysis.ClassNamingInterface(nodeContext, testAction) is { } symbol
                && !SymbolEqualityComparer.Default.Equals(symbol, testActionAttribute))
            {
                Report(nodeContext.ReportDiagnostic, symbol, testAction, attribute, attributeUsage, targetsProperty, testActionAttribute,
                    nodeContext.Node.GetLocation(), nodeContext.CancellationToken);
            }
        }, SyntaxKind.SimpleBaseType);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var symbol = (INamedTypeSymbol)symbolContext.Symbol;
            // TestActionAttribute itself is exempt by type: it is precompiled in Unity, but the Tests project compiles its dummy
            // from source, and its own AttributeUsage allows Method for a Default action.
            if (ActionAttributeAnalysis.NamesInterface(symbol, testAction)
                || SymbolEqualityComparer.Default.Equals(symbol, testActionAttribute))
            {
                return;
            }

            Report(symbolContext.ReportDiagnostic, symbol, testAction, attribute, attributeUsage, targetsProperty, testActionAttribute,
                symbol.Locations[0], symbolContext.CancellationToken);
        }, SymbolKind.NamedType);
    }

    private static void Report(Action<Diagnostic> report, INamedTypeSymbol symbol, INamedTypeSymbol testAction,
        INamedTypeSymbol attribute, INamedTypeSymbol attributeUsage, IPropertySymbol targetsProperty,
        INamedTypeSymbol? testActionAttribute, Location location, CancellationToken cancellationToken)
    {
        if (!ActionAttributeAnalysis.IsActionAttributeClass(symbol, testAction, attribute))
        {
            return;
        }

        var (supported, targetsText, supportedText) = Describe(ResolveTargets(symbol, targetsProperty, testActionAttribute, cancellationToken));
        if ((ActionAttributeAnalysis.EffectiveValidOn(symbol, attributeUsage) & ~supported) == 0)
        {
            return;
        }

        report(Diagnostic.Create(Rule, location, targetsText, supportedText));
    }

    /// <summary>
    /// The constant returned by the Targets getter the class inherits, or <see cref="NotAConstant"/>.
    /// </summary>
    private static int ResolveTargets(INamedTypeSymbol symbol, IPropertySymbol targetsProperty,
        INamedTypeSymbol? testActionAttribute, CancellationToken cancellationToken)
    {
        if (symbol.FindImplementationForInterfaceMember(targetsProperty) is not IPropertySymbol mapped)
        {
            return NotAConstant;
        }

        var implementation = MostDerivedOverride(symbol, mapped);
        if (implementation.DeclaringSyntaxReferences.IsEmpty)
        {
            return SymbolEqualityComparer.Default.Equals(implementation.ContainingType, testActionAttribute) ? ActionTargetsDefault : NotAConstant;
        }

        int? folded = null;
        foreach (var reference in implementation.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(cancellationToken) is not PropertyDeclarationSyntax property)
            {
                continue;
            }

            foreach (var returned in ReturnedExpressions(property))
            {
                var constant = Fold(returned);
                folded = folded is null ? constant : NotAConstant;
            }
        }

        return folded ?? NotAConstant;
    }

    private static IEnumerable<ExpressionSyntax> ReturnedExpressions(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody is { } arrow)
        {
            return new[] { arrow.Expression };
        }

        var getter = property.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        if (getter?.ExpressionBody is { } getterArrow)
        {
            return new[] { getterArrow.Expression };
        }

        return getter?.Body?.DescendantNodes().OfType<ReturnStatementSyntax>().Select(r => r.Expression).OfType<ExpressionSyntax>()
               ?? Enumerable.Empty<ExpressionSyntax>();
    }

    /// <summary>
    /// Folds the expression by syntax alone: ActionTargets members joined with '|', parentheses, a numeric literal, or a
    /// cast of one. Without a SemanticModel a field or a using-static reference cannot be resolved and yields <see cref="NotAConstant"/>.
    /// </summary>
    private static int Fold(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return Fold(parenthesized.Expression);
            case CastExpressionSyntax cast:
                return Fold(cast.Expression);
            case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.BitwiseOrExpression } or:
                var left = Fold(or.Left);
                var right = Fold(or.Right);
                return left == NotAConstant || right == NotAConstant ? NotAConstant : left | right;
            case LiteralExpressionSyntax { Token.Value: int literal }:
                return literal;
            case MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.ValueText: "ActionTargets" }
                or MemberAccessExpressionSyntax { Name.Identifier.ValueText: "ActionTargets" }
            } member:
                return member.Name.Identifier.ValueText switch
                {
                    "Default" => ActionTargetsDefault,
                    "Test" => ActionTargetsTest,
                    "Suite" => ActionTargetsSuite,
                    _ => NotAConstant,
                };
            default:
                return NotAConstant;
        }
    }

    /// <summary>
    /// FindImplementationForInterfaceMember returns the member of the type that declares the interface, so a derived
    /// class that overrides a virtual Targets is resolved here by walking down from the class itself.
    /// </summary>
    private static IPropertySymbol MostDerivedOverride(INamedTypeSymbol symbol, IPropertySymbol mapped)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                for (var overridden = property; overridden is not null; overridden = overridden.OverriddenProperty)
                {
                    if (SymbolEqualityComparer.Default.Equals(overridden, mapped))
                    {
                        return property;
                    }
                }
            }
        }

        return mapped;
    }

    private static (AttributeTargets Supported, string TargetsText, string SupportedText) Describe(int targets)
    {
        return targets switch
        {
            ActionTargetsTest => (AttributeTargets.Method, "'ActionTargets.Test'", "test methods"),
            ActionTargetsSuite => (SuiteTargets, "'ActionTargets.Suite'", "fixture classes, interfaces, and assemblies"),
            ActionTargetsDefault => (SuiteTargets, "'ActionTargets.Default'", "fixture classes, interfaces, and assemblies"),
            ActionTargetsTest | ActionTargetsSuite => (AttributeTargets.Method | SuiteTargets, "'ActionTargets.Test | ActionTargets.Suite'", "test methods, fixture classes, interfaces, and assemblies"),
            _ => (AttributeTargets.Method | AttributeTargets.Class, "not a constant", "test methods and fixture classes"),
        };
    }
}
