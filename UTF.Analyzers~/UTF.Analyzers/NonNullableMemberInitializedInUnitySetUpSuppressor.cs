using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF3002: Suppress CS8618 (Non-nullable field or property is uninitialized) when the member is initialized
/// in a UnitySetUp or UnityOneTimeSetUp method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonNullableMemberInitializedInUnitySetUpSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "UTF3002";
    public const string SuppressedDiagnosticId = "CS8618";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Field/Property is initialized in UnitySetUp or UnityOneTimeSetUp method.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // Looked up independently: UnityOneTimeSetUpAttribute exists only in UTF 1.5.0+, and requiring both would disable the suppressor on 1.4.x.
        var unitySetUp = context.Compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnitySetUpAttribute");
        var unityOneTimeSetUp =
            context.Compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeSetUpAttribute");
        if (unitySetUp is null && unityOneTimeSetUp is null)
        {
            return;
        }

        // Every CS8618 in a class shares the same setup methods, so they are resolved once per class.
        var setUpMethodsByClass = new Dictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>>();

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var tree = diagnostic.Location.SourceTree;
            if (tree is null)
            {
                continue;
            }

            // Constructor-located CS8618 is left alone: the member name is available only via Roslyn's internal diagnostic arguments.
            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not (VariableDeclaratorSyntax or PropertyDeclarationSyntax) ||
                node.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not { } classDeclaration)
            {
                continue;
            }

            var model = context.GetSemanticModel(tree);
            if (model.GetDeclaredSymbol(node, context.CancellationToken) is not { } member)
            {
                continue;
            }

            if (!setUpMethodsByClass.TryGetValue(classDeclaration, out var setUpMethods))
            {
                setUpMethods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                    .Where(method =>
                        UnityHookMethodAnalysis.HasAnyAttribute(
                            model.GetDeclaredSymbol(method, context.CancellationToken) as IMethodSymbol, unitySetUp,
                            unityOneTimeSetUp))
                    .ToList();
                setUpMethodsByClass.Add(classDeclaration, setUpMethods);
            }

            if (setUpMethods.Any(method => new AssignmentWalker(model, member).IsAssignedIn(method)))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }

    /// <summary>
    /// Finds an unconditional assignment to one member, following calls into methods of the same type.
    /// </summary>
    private sealed class AssignmentWalker
    {
        private readonly SemanticModel _model;
        private readonly ISymbol _member;
        private readonly HashSet<MethodDeclarationSyntax> _visited = new();

        public AssignmentWalker(SemanticModel model, ISymbol member)
        {
            _model = model;
            _member = member;
        }

        public bool IsAssignedIn(MethodDeclarationSyntax method)
        {
            if (!_visited.Add(method))
            {
                return false;
            }

            if (method.ExpressionBody is not null)
            {
                return IsAssignedIn(method.ExpressionBody.Expression);
            }

            return method.Body is not null && IsAssignedIn(method.Body.Statements);
        }

        private bool IsAssignedIn(SyntaxList<StatementSyntax> statements)
        {
            // Conditional statements and loops do not guarantee the assignment, so only unconditional forms are followed.
            return statements.Any(statement => statement switch
            {
                ExpressionStatementSyntax expression => IsAssignedIn(expression.Expression),
                BlockSyntax block => IsAssignedIn(block.Statements),
                TryStatementSyntax @try => IsAssignedIn(@try.Block.Statements) ||
                                           (@try.Finally is not null && IsAssignedIn(@try.Finally.Block.Statements)),
                _ => false,
            });
        }

        private bool IsAssignedIn(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case AssignmentExpressionSyntax { Left: TupleExpressionSyntax tuple }:
                    return tuple.Arguments.Any(argument => IsMember(argument.Expression));
                case AssignmentExpressionSyntax assignment:
                    return IsMember(assignment.Left);
                case InvocationExpressionSyntax invocation:
                    // Only methods of the same type are followed; the containing-type check comes first because it is cheaper than realizing the syntax.
                    return _model.GetSymbolInfo(invocation).Symbol is IMethodSymbol callee &&
                           SymbolEqualityComparer.Default.Equals(callee.ContainingType, _member.ContainingType) &&
                           callee.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax
                               declaration &&
                           IsAssignedIn(declaration);
                default:
                    return false;
            }
        }

        private bool IsMember(ExpressionSyntax expression)
        {
            // Comparing symbols rather than identifier text keeps a local or parameter with the member's name from counting.
            return SymbolEqualityComparer.Default.Equals(_model.GetSymbolInfo(expression).Symbol, _member);
        }
    }
}
