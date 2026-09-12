using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5004.md");

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

        var targetsProperty = FindTargetsProperty(testAction);
        if (targetsProperty is null)
        {
            return;
        }

        var testActionAttribute = compilation.GetTypeByMetadataName("NUnit.Framework.TestActionAttribute");

        // Everything is reported at compilation end: the Targets value of a class comes from the getter it inherits, which may
        // be declared in another file, and the constant is only known once that getter's operation block has been analyzed.
        // A symbol-start action per type cannot wait for another type's block. The cost is that the diagnostics appear on
        // build and on full-solution analysis, not in the IDE's open-files mode.
        var targetsByGetter = new ConcurrentDictionary<IMethodSymbol, int>(SymbolEqualityComparer.Default);
        var entryByType = new ConcurrentDictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
        var candidates = new ConcurrentBag<INamedTypeSymbol>();

        context.RegisterOperationBlockStartAction(blockContext =>
        {
            blockContext.CancellationToken.ThrowIfCancellationRequested();
            if (blockContext.OwningSymbol is not IMethodSymbol { MethodKind: MethodKind.PropertyGet, AssociatedSymbol: IPropertySymbol property } getter
                || !ImplementsTargets(property, targetsProperty))
            {
                return;
            }

            var returned = new ConstantCollector();
            blockContext.RegisterOperationAction(operationContext =>
            {
                var value = ((IReturnOperation)operationContext.Operation).ReturnedValue?.ConstantValue;
                returned.Add(value is { HasValue: true, Value: int constant } ? constant : NotAConstant);
            }, OperationKind.Return);
            blockContext.RegisterOperationBlockEndAction(_ => targetsByGetter[getter] = returned.Value);
        });

        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            var entry = (SimpleBaseTypeSyntax)nodeContext.Node;
            if (entry.Parent?.Parent is not ClassDeclarationSyntax classDeclaration
                || nodeContext.SemanticModel.GetTypeInfo(entry.Type, nodeContext.CancellationToken).Type is not
                    INamedTypeSymbol { TypeKind: TypeKind.Interface } named
                || !ActionAttributeAnalysis.IsOrDerivesFrom(named, testAction)
                || nodeContext.SemanticModel.GetDeclaredSymbol(classDeclaration, nodeContext.CancellationToken) is not { } symbol)
            {
                return;
            }

            entryByType[symbol] = entry.GetLocation();
        }, SyntaxKind.SimpleBaseType);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var symbol = (INamedTypeSymbol)symbolContext.Symbol;
            // TestActionAttribute itself is exempt by type: it is precompiled in Unity, but the Tests project compiles its dummy
            // from source, and its own AttributeUsage allows Method for a Default action.
            if (symbol.TypeKind == TypeKind.Class
                && !SymbolEqualityComparer.Default.Equals(symbol, testActionAttribute)
                && ActionAttributeAnalysis.DerivesFrom(symbol, attribute)
                && ActionAttributeAnalysis.Implements(symbol, testAction))
            {
                candidates.Add(symbol);
            }
        }, SymbolKind.NamedType);

        context.RegisterCompilationEndAction(endContext =>
        {
            foreach (var symbol in candidates)
            {
                endContext.CancellationToken.ThrowIfCancellationRequested();
                var targets = ResolveTargets(symbol, targetsProperty, targetsByGetter, testActionAttribute);
                var supported = targets switch
                {
                    ActionTargetsTest => AttributeTargets.Method,
                    ActionTargetsSuite or ActionTargetsDefault => SuiteTargets,
                    ActionTargetsTest | ActionTargetsSuite => AttributeTargets.Method | SuiteTargets,
                    _ => AttributeTargets.Method | AttributeTargets.Class,
                };
                if ((ActionAttributeAnalysis.EffectiveValidOn(symbol, attributeUsage) & ~supported) == 0)
                {
                    continue;
                }

                var location = entryByType.TryGetValue(symbol, out var entry) ? entry : symbol.Locations[0];
                endContext.ReportDiagnostic(Diagnostic.Create(Rule, location, Describe(targets), Describe(supported)));
            }
        });
    }

    private static IPropertySymbol? FindTargetsProperty(INamedTypeSymbol testAction)
    {
        foreach (var member in testAction.GetMembers("Targets"))
        {
            if (member is IPropertySymbol property)
            {
                return property;
            }
        }

        return null;
    }

    private static bool ImplementsTargets(IPropertySymbol property, IPropertySymbol targetsProperty)
    {
        if (property.ExplicitInterfaceImplementations.Length > 0)
        {
            foreach (var explicitImplementation in property.ExplicitInterfaceImplementations)
            {
                if (SymbolEqualityComparer.Default.Equals(explicitImplementation, targetsProperty))
                {
                    return true;
                }
            }

            return false;
        }

        // Name and type instead of FindImplementationForInterfaceMember: an abstract attribute class may declare Targets
        // without naming ITestAction itself, and a derived class then overrides it.
        return property.Name == targetsProperty.Name
               && SymbolEqualityComparer.Default.Equals(property.Type, targetsProperty.Type);
    }

    /// <summary>
    /// The constant returned by the Targets getter the class inherits, or <see cref="NotAConstant"/>.
    /// </summary>
    private static int ResolveTargets(INamedTypeSymbol symbol, IPropertySymbol targetsProperty,
        ConcurrentDictionary<IMethodSymbol, int> targetsByGetter, INamedTypeSymbol? testActionAttribute)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            foreach (var member in type.GetMembers())
            {
                if (member is not IPropertySymbol { GetMethod: { } getter } property || !ImplementsTargets(property, targetsProperty))
                {
                    continue;
                }

                if (targetsByGetter.TryGetValue(getter, out var constant))
                {
                    return constant;
                }

                return SymbolEqualityComparer.Default.Equals(type, testActionAttribute) ? ActionTargetsDefault : NotAConstant;
            }
        }

        return NotAConstant;
    }

    private static string Describe(int targets)
    {
        return targets switch
        {
            ActionTargetsDefault => "'ActionTargets.Default'",
            ActionTargetsTest => "'ActionTargets.Test'",
            ActionTargetsSuite => "'ActionTargets.Suite'",
            ActionTargetsTest | ActionTargetsSuite => "'ActionTargets.Test | ActionTargets.Suite'",
            _ => "not a constant",
        };
    }

    private static string Describe(AttributeTargets supported)
    {
        return supported switch
        {
            AttributeTargets.Method => "test methods",
            SuiteTargets => "fixture classes, interfaces, and assemblies",
            AttributeTargets.Method | SuiteTargets => "test methods, fixture classes, interfaces, and assemblies",
            _ => "test methods and fixture classes",
        };
    }

    /// <summary>
    /// Folds the returns of one getter: a single constant survives, anything else becomes <see cref="NotAConstant"/>.
    /// Operation actions of one block may run concurrently, hence the lock.
    /// </summary>
    private sealed class ConstantCollector
    {
        private readonly object _lock = new();
        private int _value = NotAConstant;
        private bool _seen;

        public int Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
        }

        public void Add(int constant)
        {
            lock (_lock)
            {
                _value = _seen ? NotAConstant : constant;
                _seen = true;
            }
        }
    }
}
