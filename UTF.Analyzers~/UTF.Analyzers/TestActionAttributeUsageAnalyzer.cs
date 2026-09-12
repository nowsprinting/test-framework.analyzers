using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5004.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private const int ActionTargetsDefault = 0;
    private const int ActionTargetsTest = 1;
    private const int ActionTargetsSuite = 2;
    private const AttributeTargets SuiteTargets = AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly;

    /// <summary>
    /// Sentinel for a Targets getter whose returned value is not a single compile-time constant.
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

        // Everything is reported at compilation end: the Targets value of a class comes from the getter it inherits, which may
        // be declared in another file, and the constant is only known once that getter's operation block has been analyzed.
        // A symbol-start action per type cannot wait for another type's block. The cost is that the diagnostics appear on
        // build and on full-solution analysis, not in the IDE's open-files mode.
        var targetsByProperty = new ConcurrentDictionary<IPropertySymbol, int>(SymbolEqualityComparer.Default);
        var entryByType = new ConcurrentDictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
        var candidates = new ConcurrentBag<INamedTypeSymbol>();

        context.RegisterOperationBlockAction(blockContext =>
        {
            blockContext.CancellationToken.ThrowIfCancellationRequested();
            if (blockContext.OwningSymbol is not IMethodSymbol { MethodKind: MethodKind.PropertyGet, AssociatedSymbol: IPropertySymbol property }
                || !SymbolEqualityComparer.Default.Equals(property.Type, targetsProperty.Type))
            {
                return;
            }

            int? folded = null;
            foreach (var block in blockContext.OperationBlocks)
            {
                foreach (var returned in block.DescendantsAndSelf().OfType<IReturnOperation>())
                {
                    var value = returned.ReturnedValue?.ConstantValue;
                    var constant = value is { HasValue: true, Value: int i } ? i : NotAConstant;
                    folded = folded is null ? constant : NotAConstant;
                }
            }

            targetsByProperty[property] = folded ?? NotAConstant;
        });

        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            if (ActionAttributeAnalysis.ClassNamingInterface(nodeContext, testAction) is { } symbol)
            {
                entryByType[symbol] = nodeContext.Node.GetLocation();
            }
        }, SyntaxKind.SimpleBaseType);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var symbol = (INamedTypeSymbol)symbolContext.Symbol;
            // TestActionAttribute itself is exempt by type: it is precompiled in Unity, but the Tests project compiles its dummy
            // from source, and its own AttributeUsage allows Method for a Default action.
            if (!SymbolEqualityComparer.Default.Equals(symbol, testActionAttribute)
                && ActionAttributeAnalysis.IsActionAttributeClass(symbol, testAction, attribute))
            {
                candidates.Add(symbol);
            }
        }, SymbolKind.NamedType);

        context.RegisterCompilationEndAction(endContext =>
        {
            foreach (var symbol in candidates)
            {
                endContext.CancellationToken.ThrowIfCancellationRequested();
                var (supported, targetsText, supportedText) = Describe(ResolveTargets(symbol, targetsProperty, targetsByProperty, testActionAttribute));
                if ((ActionAttributeAnalysis.EffectiveValidOn(symbol, attributeUsage) & ~supported) == 0)
                {
                    continue;
                }

                var location = entryByType.TryGetValue(symbol, out var entry) ? entry : symbol.Locations[0];
                endContext.ReportDiagnostic(Diagnostic.Create(Rule, location, targetsText, supportedText));
            }
        });
    }

    /// <summary>
    /// The constant returned by the Targets getter the class inherits, or <see cref="NotAConstant"/>.
    /// </summary>
    private static int ResolveTargets(INamedTypeSymbol symbol, IPropertySymbol targetsProperty,
        ConcurrentDictionary<IPropertySymbol, int> targetsByProperty, INamedTypeSymbol? testActionAttribute)
    {
        if (symbol.FindImplementationForInterfaceMember(targetsProperty) is not IPropertySymbol mapped)
        {
            return NotAConstant;
        }

        var implementation = MostDerivedOverride(symbol, mapped);
        return targetsByProperty.TryGetValue(implementation, out var constant) ? constant
            : SymbolEqualityComparer.Default.Equals(implementation.ContainingType, testActionAttribute) ? ActionTargetsDefault
            : NotAConstant;
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
