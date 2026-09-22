using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
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
        messageFormat:
        "'{0}' is declared in a test assembly and appears in the Add Component menu of the Unity Editor, where it can be attached to a scene object by mistake. Apply AddComponentMenu with a menu name that starts with '/' to hide it.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a class deriving from UnityEngine.MonoBehaviour that is declared in a test assembly (assembly name ending with .Tests, or a source file under a Tests directory) and is not hidden from the Unity Editor's Add Component menu with [AddComponentMenu(\"/\")]. Test doubles that derive from MonoBehaviour exist only to be attached from test code, but the Editor lists every MonoBehaviour in the picker, so they show up next to the production components.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4005.md");

    private const string TestsAssemblySuffix = ".Tests";
    private const string TestsDirectory = "/Tests/";

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

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var symbol = (INamedTypeSymbol)symbolContext.Symbol;
            if (symbol.TypeKind != TypeKind.Class
                || symbol.IsAbstract
                || symbol.IsGenericType
                || symbol.ContainingType is not null
                || !ActionAttributeAnalysis.DerivesFrom(symbol, monoBehaviour))
            {
                return;
            }

            // The attribute check comes after the path and file-name checks: GetAttributes binds the attribute
            // constructors, and in an assembly that is not a test assembly no class ever needs it.
            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                var path = reference.SyntaxTree.FilePath;
                if ((assemblyIsTests || IsUnderTestsDirectory(path))
                    && IsResolvedAsComponent(reference, symbol.Name, symbolContext.CancellationToken))
                {
                    if (IsHidden(symbol, addComponentMenu))
                    {
                        return;
                    }

                    var declaration = (ClassDeclarationSyntax)reference.GetSyntax(symbolContext.CancellationToken);
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(),
                        symbol.Name));
                    return;
                }
            }
        }, SymbolKind.NamedType);
    }

    private static bool IsUnderTestsDirectory(string filePath)
    {
        // Normalized by hand rather than through Path: the analyzer runs on macOS against paths that Unity on Windows
        // wrote with backslashes, and Path treats those as part of the file name there.
        return filePath.Replace('\\', '/').Contains(TestsDirectory);
    }

    /// <summary>
    /// Unity resolves the component class of a script as the one named after the file, or the sole top-level type of the file.
    /// </summary>
    private static bool IsResolvedAsComponent(SyntaxReference reference, string className,
        CancellationToken cancellationToken)
    {
        var tree = reference.SyntaxTree;
        if (string.Equals(Path.GetFileNameWithoutExtension(tree.FilePath), className, StringComparison.Ordinal))
        {
            return true;
        }

        var topLevelTypes = 0;
        foreach (var node in tree.GetRoot(cancellationToken)
                     .DescendantNodes(descendIntoChildren: node =>
                         node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax && ++topLevelTypes > 1)
            {
                return false;
            }
        }

        return topLevelTypes == 1;
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
