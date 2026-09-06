using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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
        var setUpAttributes = new[]
            {
                context.Compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnitySetUpAttribute"),
                context.Compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeSetUpAttribute"),
            }
            .Where(type => type is not null)
            .Select(type => type!)
            .ToImmutableArray();
        if (setUpAttributes.IsEmpty)
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

            // Constructor-located CS8618 is left alone: the member name is available only via Roslyn's internal diagnostic arguments.
            var node = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            var memberName = node switch
            {
                VariableDeclaratorSyntax declarator => declarator.Identifier.Text,
                PropertyDeclarationSyntax property => property.Identifier.Text,
                _ => null,
            };
            if (memberName is null || node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault() is not { } classDeclaration)
            {
                continue;
            }

            var model = context.GetSemanticModel(tree);
            foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                if (IsUnitySetUp(model.GetDeclaredSymbol(method, context.CancellationToken) as IMethodSymbol, setUpAttributes) &&
                    IsAssignedIn(model, classDeclaration, new HashSet<MethodDeclarationSyntax>(), method, memberName))
                {
                    context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                    break;
                }
            }
        }
    }

    private static bool IsUnitySetUp(IMethodSymbol? method, ImmutableArray<INamedTypeSymbol> setUpAttributes)
    {
        // The attribute may sit on a base declaration that this method overrides.
        for (; method is not null; method = method.OverriddenMethod)
        {
            if (method.GetAttributes().Any(attribute =>
                    setUpAttributes.Contains(attribute.AttributeClass?.OriginalDefinition!, SymbolEqualityComparer.Default)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAssignedIn(SemanticModel model, ClassDeclarationSyntax classDeclaration,
        HashSet<MethodDeclarationSyntax> visited, MethodDeclarationSyntax method, string memberName)
    {
        if (!visited.Add(method))
        {
            return false;
        }

        if (method.ExpressionBody is not null)
        {
            return IsAssignedIn(model, classDeclaration, visited, method.ExpressionBody.Expression, memberName);
        }

        return method.Body is not null && IsAssignedIn(model, classDeclaration, visited, method.Body.Statements, memberName);
    }

    private static bool IsAssignedIn(SemanticModel model, ClassDeclarationSyntax classDeclaration,
        HashSet<MethodDeclarationSyntax> visited, SyntaxList<StatementSyntax> statements, string memberName)
    {
        foreach (var statement in statements)
        {
            // Conditional statements and loops do not guarantee the assignment, so only unconditional forms are followed.
            var assigned = statement switch
            {
                ExpressionStatementSyntax expression => IsAssignedIn(model, classDeclaration, visited, expression.Expression, memberName),
                BlockSyntax block => IsAssignedIn(model, classDeclaration, visited, block.Statements, memberName),
                TryStatementSyntax @try => IsAssignedIn(model, classDeclaration, visited, @try.Block.Statements, memberName) ||
                                           (@try.Finally is not null && IsAssignedIn(model, classDeclaration, visited, @try.Finally.Block.Statements, memberName)),
                _ => false,
            };
            if (assigned)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAssignedIn(SemanticModel model, ClassDeclarationSyntax classDeclaration,
        HashSet<MethodDeclarationSyntax> visited, ExpressionSyntax expression, string memberName)
    {
        switch (expression)
        {
            case AssignmentExpressionSyntax { Left: TupleExpressionSyntax tuple }:
                return tuple.Arguments.Any(argument => GetIdentifier(argument.Expression) == memberName);
            case AssignmentExpressionSyntax assignment:
                return GetIdentifier(assignment.Left) == memberName;
            case InvocationExpressionSyntax invocation:
                // Only methods declared in the same class are followed; their bodies are the only ones guaranteed to be in this compilation.
                return model.GetSymbolInfo(invocation).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax callee &&
                       callee.Parent == classDeclaration &&
                       IsAssignedIn(model, classDeclaration, visited, callee, memberName);
            default:
                return false;
        }
    }

    private static string? GetIdentifier(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } access => access.Name.Identifier.Text,
            _ => null,
        };
    }
}
