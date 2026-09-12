using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF5002: Implementing IWrapTestMethod and IWrapSetUpTearDown is not recommended.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandWrapperImplementationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF5002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Implementing IWrapTestMethod and IWrapSetUpTearDown is not recommended",
        messageFormat: "Attributes implementing '{0}' cannot be applied to async and coroutine-style test methods. Implement IOuterUnityTestAction instead.",
        category: "Extensions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a class whose base list names NUnit.Framework.Interfaces.IWrapTestMethod or NUnit.Framework.Interfaces.IWrapSetUpTearDown. Unity Test Framework cannot run the command returned by a user-defined Wrap on async Task and coroutine-style test methods, so such an attribute can be applied only to synchronous test methods. UnityEngine.TestTools.IOuterUnityTestAction provides before/after hooks that work on every kind of test method.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF5002.md");

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
        var wrapTestMethod = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.IWrapTestMethod");
        var wrapSetUpTearDown = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.IWrapSetUpTearDown");
        if (wrapTestMethod is null && wrapSetUpTearDown is null)
        {
            return;
        }

        // The analyzer is attached to UnityEngine.TestRunner through an .asmref, so ParametrizedIgnoreAttribute, compiled
        // from the package source, would be reported in every project. The NUnit attributes are precompiled and never
        // inspected in Unity; they are listed so that the exemption agrees with UTF1005.
        // A null entry (type not referenced) never equals a class symbol, so no filtering is needed.
        var exempt = new[]
        {
            compilation.GetTypeByMetadataName("NUnit.Framework.RepeatAttribute"),
            compilation.GetTypeByMetadataName("NUnit.Framework.RetryAttribute"),
            compilation.GetTypeByMetadataName("NUnit.Framework.MaxTimeAttribute"),
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.ParametrizedIgnoreAttribute"),
        };

        // A syntax node action on the base list entry rather than a symbol action on the class: the location is the entry
        // that names the interface, and a symbol action would have to re-resolve every base type syntax of every declaration.
        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            var baseType = (SimpleBaseTypeSyntax)nodeContext.Node;
            if (baseType.Parent?.Parent is not ClassDeclarationSyntax classDeclaration)
            {
                return;
            }

            if (nodeContext.SemanticModel.GetTypeInfo(baseType.Type, nodeContext.CancellationToken).Type is not
                INamedTypeSymbol { TypeKind: TypeKind.Interface } named)
            {
                return;
            }

            if (!IsOrDerivesFrom(named, wrapTestMethod) && !IsOrDerivesFrom(named, wrapSetUpTearDown))
            {
                return;
            }

            var declared = nodeContext.SemanticModel.GetDeclaredSymbol(classDeclaration, nodeContext.CancellationToken);
            if (exempt.Contains(declared, SymbolEqualityComparer.Default))
            {
                return;
            }

            nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, baseType.GetLocation(), named.Name));
        }, SyntaxKind.SimpleBaseType);
    }

    private static bool IsOrDerivesFrom(INamedTypeSymbol candidate, INamedTypeSymbol? target)
    {
        return target is not null
               && (SymbolEqualityComparer.Default.Equals(candidate, target)
                   || candidate.AllInterfaces.Contains(target, SymbolEqualityComparer.Default));
    }
}
