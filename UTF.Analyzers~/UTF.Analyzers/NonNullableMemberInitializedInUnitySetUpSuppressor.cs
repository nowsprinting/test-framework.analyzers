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

        // The setup methods of a class are walked once and the members they assign are kept as a set, so each CS8618 of
        // that class is a lookup; walking the setup methods again for each member would cost the size of the setup
        // code per uninitialized member.
        var assignedByClass = new Dictionary<ClassDeclarationSyntax, HashSet<ISymbol>>();

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

            if (!assignedByClass.TryGetValue(classDeclaration, out var assigned))
            {
                assigned = AssignedMembers(context, model, classDeclaration, unitySetUp, unityOneTimeSetUp);
                assignedByClass.Add(classDeclaration, assigned);
            }

            if (assigned.Contains(member))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }

    private static HashSet<ISymbol> AssignedMembers(SuppressionAnalysisContext context, SemanticModel model,
        ClassDeclarationSyntax classDeclaration, INamedTypeSymbol? unitySetUp, INamedTypeSymbol? unityOneTimeSetUp)
    {
        var walker = new AssignmentWalker(context,
            model.GetDeclaredSymbol(classDeclaration, context.CancellationToken) as INamedTypeSymbol);
        foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            if (UnityHookMethodAnalysis.HasAnyAttribute(
                    model.GetDeclaredSymbol(method, context.CancellationToken) as IMethodSymbol, unitySetUp,
                    unityOneTimeSetUp))
            {
                walker.Visit(method);
            }
        }

        return walker.Assigned;
    }

    /// <summary>
    /// Collects the members of one type that are unconditionally assigned, following calls into methods of the same type.
    /// </summary>
    private sealed class AssignmentWalker
    {
        private readonly SuppressionAnalysisContext _context;
        private readonly INamedTypeSymbol? _type;
        private readonly HashSet<MethodDeclarationSyntax> _visited = new();

        public HashSet<ISymbol> Assigned { get; } = new(SymbolEqualityComparer.Default);

        public AssignmentWalker(SuppressionAnalysisContext context, INamedTypeSymbol? type)
        {
            _context = context;
            _type = type;
        }

        public void Visit(MethodDeclarationSyntax method)
        {
            if (!_visited.Add(method))
            {
                return;
            }

            if (method.ExpressionBody is not null)
            {
                Visit(method.ExpressionBody.Expression);
            }
            else if (method.Body is not null)
            {
                Visit(method.Body.Statements);
            }
        }

        private void Visit(SyntaxList<StatementSyntax> statements)
        {
            // Conditional statements and loops do not guarantee the assignment, so only unconditional forms are followed.
            foreach (var statement in statements)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                switch (statement)
                {
                    case ExpressionStatementSyntax expression:
                        Visit(expression.Expression);
                        break;
                    case BlockSyntax block:
                        Visit(block.Statements);
                        break;
                    case TryStatementSyntax @try:
                        Visit(@try.Block.Statements);
                        if (@try.Finally is not null)
                        {
                            Visit(@try.Finally.Block.Statements);
                        }

                        break;
                }
            }
        }

        private void Visit(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case AssignmentExpressionSyntax { Left: TupleExpressionSyntax tuple }:
                    foreach (var argument in tuple.Arguments)
                    {
                        AddMember(argument.Expression);
                    }

                    break;
                case AssignmentExpressionSyntax assignment:
                    AddMember(assignment.Left);
                    break;
                case InvocationExpressionSyntax invocation:
                    // Only methods of the same type are followed; the containing-type check comes first because it is cheaper than realizing the syntax.
                    if (Model(invocation).GetSymbolInfo(invocation, _context.CancellationToken).Symbol is IMethodSymbol callee &&
                        SymbolEqualityComparer.Default.Equals(callee.ContainingType, _type) &&
                        callee.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(_context.CancellationToken) is
                            MethodDeclarationSyntax declaration)
                    {
                        Visit(declaration);
                    }

                    break;
            }
        }

        private void AddMember(ExpressionSyntax expression)
        {
            // Comparing symbols rather than identifier text keeps a local or parameter with the member's name from counting;
            // the set holds whatever the left side binds to, and a local never matches a CS8618 member at lookup.
            if (Model(expression).GetSymbolInfo(expression, _context.CancellationToken).Symbol is { } symbol)
            {
                Assigned.Add(symbol);
            }
        }

        // A followed callee may be declared in another part of a partial class, whose nodes belong to another tree;
        // asking the semantic model of the first tree about them throws.
        private SemanticModel Model(SyntaxNode node) => _context.GetSemanticModel(node.SyntaxTree);
    }
}
