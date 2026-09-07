using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF3003: Suppress CA1001 (Types that own disposable fields should be disposable) on a test fixture
/// that declares a UnityTearDown or UnityOneTimeTearDown method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableFieldDisposedInUnityTearDownSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3003";
    public const string SuppressedDiagnosticId = "CA1001";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Field should be Disposed in UnityTearDown or UnityOneTimeTearDown method.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // Looked up independently: UnityOneTimeTearDownAttribute exists only in UTF 1.5.0+, and requiring both would disable the suppressor on 1.4.x.
        var unityTearDown = context.Compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityTearDownAttribute");
        var unityOneTimeTearDown = context.Compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeTearDownAttribute");
        if (unityTearDown is null && unityOneTimeTearDown is null)
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

            // NetAnalyzers reports at the class identifier, whose enclosing node is the class declaration.
            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not ClassDeclarationSyntax classDeclaration)
            {
                continue;
            }

            // Symbol members rather than syntax members, so a teardown method in another part of a partial class counts.
            var type = context.GetSemanticModel(tree).GetDeclaredSymbol(classDeclaration, context.CancellationToken) as INamedTypeSymbol;
            if (type is not null && type.GetMembers().OfType<IMethodSymbol>().Any(method =>
                    UnityHookMethodAnalysis.HasEitherAttribute(method, unityTearDown, unityOneTimeTearDown)))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }
}
