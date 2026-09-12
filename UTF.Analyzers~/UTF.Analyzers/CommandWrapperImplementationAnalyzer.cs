using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

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
        if (wrapTestMethod is null || wrapSetUpTearDown is null)
        {
            return;
        }

        // The supported wrappers are exempt because this package attaches the analyzer to UnityEngine.TestRunner through
        // an .asmref, so ParametrizedIgnoreAttribute, compiled from the package source, would otherwise be reported in every project.
        var exempt = CommandWrapperAnalysis.SupportedWrapperAttributes(compilation);

        // A syntax node action on the base list entry rather than a symbol action on the class: the location is the entry
        // that names the interface, and locating it from a symbol action needs Compilation.GetSemanticModel (RS1030).
        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            var baseType = (SimpleBaseTypeSyntax)nodeContext.Node;
            if (baseType.Parent?.Parent is not ClassDeclarationSyntax classDeclaration)
            {
                return;
            }

            // The interface check excludes the base class entry: a class that inherits the interface from its base class
            // does not name it, and the base class is reported instead.
            if (nodeContext.SemanticModel.GetTypeInfo(baseType.Type, nodeContext.CancellationToken).Type is not
                    INamedTypeSymbol { TypeKind: TypeKind.Interface } named
                || !IsWrapperInterface(named, wrapTestMethod, wrapSetUpTearDown))
            {
                return;
            }

            var declared = nodeContext.SemanticModel.GetDeclaredSymbol(classDeclaration, nodeContext.CancellationToken);
            if (declared is not null && exempt.Contains(declared))
            {
                return;
            }

            nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, baseType.GetLocation(), named.Name));
        }, SyntaxKind.SimpleBaseType);
    }

    private static bool IsWrapperInterface(INamedTypeSymbol candidate, INamedTypeSymbol wrapTestMethod,
        INamedTypeSymbol wrapSetUpTearDown)
    {
        if (IsEither(candidate, wrapTestMethod, wrapSetUpTearDown))
        {
            return true;
        }

        // A single pass over AllInterfaces instead of two LINQ Contains calls: every base list entry in the compilation
        // reaches here, and the LINQ overloads box the ImmutableArray on each call.
        foreach (var inherited in candidate.AllInterfaces)
        {
            if (IsEither(inherited, wrapTestMethod, wrapSetUpTearDown))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEither(INamedTypeSymbol candidate, INamedTypeSymbol first, INamedTypeSymbol second)
    {
        return SymbolEqualityComparer.Default.Equals(candidate, first)
               || SymbolEqualityComparer.Default.Equals(candidate, second);
    }
}
