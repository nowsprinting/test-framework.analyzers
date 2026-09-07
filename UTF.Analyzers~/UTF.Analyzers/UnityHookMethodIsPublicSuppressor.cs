using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF3004: Suppress NUnit1028 (The non-test method is public) on a method marked with UnitySetUp, UnityOneTimeSetUp,
/// UnityTearDown, or UnityOneTimeTearDown.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnityHookMethodIsPublicSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3004";
    public const string SuppressedDiagnosticId = "NUnit1028";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Method is a UnitySetUp, UnityOneTimeSetUp, UnityTearDown, or UnityOneTimeTearDown method.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // Looked up independently: the UnityOneTime* attributes exist only in UTF 1.5.0+, and requiring all four would disable the suppressor on 1.4.x.
        var compilation = context.Compilation;
        var unitySetUp = compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnitySetUpAttribute");
        var unityOneTimeSetUp = compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeSetUpAttribute");
        var unityTearDown = compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityTearDownAttribute");
        var unityOneTimeTearDown =
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeTearDownAttribute");
        if (unitySetUp is null && unityOneTimeSetUp is null && unityTearDown is null && unityOneTimeTearDown is null)
        {
            return;
        }

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var tree = diagnostic.Location.SourceTree;
            if (tree is null)
            {
                continue;
            }

            // NUnit.Analyzers reports at the method identifier, whose enclosing node is the method declaration.
            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            var method =
                context.GetSemanticModel(tree).GetDeclaredSymbol(methodDeclaration, context.CancellationToken) as
                    IMethodSymbol;
            if (UnityHookMethodAnalysis.HasAnyAttribute(method, unitySetUp, unityOneTimeSetUp, unityTearDown,
                    unityOneTimeTearDown))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }
}
