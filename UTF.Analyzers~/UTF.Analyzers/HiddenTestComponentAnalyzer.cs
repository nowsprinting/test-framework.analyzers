using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4005: MonoBehaviour classes in test assemblies should be hidden from the Add Component menu.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HiddenTestComponentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4005";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "MonoBehaviour classes in test assemblies should be hidden from the Add Component menu",
        messageFormat: "'{0}' is declared in a test assembly and appears in the Add Component menu of the Unity Editor, where it can be attached to a scene object by mistake. Apply AddComponentMenu with a menu name that starts with '/' to hide it.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a class deriving from UnityEngine.MonoBehaviour that is declared in a test assembly (assembly name ending with .Tests, or a source file under a Tests directory) and is not hidden from the Unity Editor's Add Component menu with [AddComponentMenu(\"/\")]. Test doubles that derive from MonoBehaviour exist only to be attached from test code, but the Editor lists every MonoBehaviour in the picker, so they show up next to the production components.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4005.md");

    private const string TestsAssemblySuffix = ".Tests";
    private const string TestsDirectory = "Tests";

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
        var monoBehaviour = compilation.GetTypeByMetadataName("UnityEngine.MonoBehaviour");
        var addComponentMenu = compilation.GetTypeByMetadataName("UnityEngine.AddComponentMenu");
        if (monoBehaviour is null || addComponentMenu is null)
        {
            return;
        }

        var assemblyIsTests = compilation.AssemblyName?.EndsWith(TestsAssemblySuffix, StringComparison.Ordinal) == true;

        // A syntax action rather than a symbol action: the file-name test is per declaration, and a partial class
        // is reported at the declaration that passes it, which a symbol action would have to search for.
        context.RegisterSyntaxNodeAction(nodeContext =>
        {
            nodeContext.CancellationToken.ThrowIfCancellationRequested();
            var declaration = (ClassDeclarationSyntax)nodeContext.Node;
            if (declaration.Parent is TypeDeclarationSyntax
                || declaration.TypeParameterList is not null
                || declaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
                || !(assemblyIsTests || IsUnderTestsDirectory(declaration.SyntaxTree.FilePath))
                || !IsResolvedAsComponent(declaration))
            {
                return;
            }

            var symbol = nodeContext.SemanticModel.GetDeclaredSymbol(declaration, nodeContext.CancellationToken);
            if (symbol is null
                || !ActionAttributeAnalysis.DerivesFrom(symbol, monoBehaviour)
                || IsHidden(symbol, addComponentMenu)
                || !IsFirstDeclarationInFile(symbol, declaration))
            {
                return;
            }

            nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), symbol.Name));
        }, SyntaxKind.ClassDeclaration);
    }

    private static bool IsUnderTestsDirectory(string filePath)
    {
        // Split on both separators instead of Path.GetDirectoryName: the analyzer runs on macOS against paths
        // that Unity on Windows wrote with backslashes, and Path treats those as part of the file name there.
        var segments = filePath.Split('/', '\\');
        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i] == TestsDirectory)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Unity resolves the component class of a script as the sole top-level type of the file, or the one named after the file.
    /// </summary>
    private static bool IsResolvedAsComponent(ClassDeclarationSyntax declaration)
    {
        var root = declaration.SyntaxTree.GetRoot();
        var topLevelTypes = 0;
        foreach (var node in root.DescendantNodes(descendIntoChildren: node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
            {
                topLevelTypes++;
            }
        }

        return topLevelTypes == 1
               || string.Equals(Path.GetFileNameWithoutExtension(declaration.SyntaxTree.FilePath), declaration.Identifier.ValueText, StringComparison.Ordinal);
    }

    /// <summary>
    /// A partial class with several declarations in the file named after it is reported once, at the first.
    /// </summary>
    private static bool IsFirstDeclarationInFile(INamedTypeSymbol symbol, ClassDeclarationSyntax declaration)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (reference.SyntaxTree == declaration.SyntaxTree)
            {
                return reference.Span == declaration.Span;
            }
        }

        return false;
    }

    private static bool IsHidden(INamedTypeSymbol symbol, INamedTypeSymbol addComponentMenu)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, addComponentMenu)
                && attribute.ConstructorArguments.Length > 0
                && attribute.ConstructorArguments[0].Value is string menuName
                && menuName.StartsWith("/", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
