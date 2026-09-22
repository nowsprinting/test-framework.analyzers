using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace UTF.Analyzers.Utilities;

internal static class OperationAnalysis
{
    /// <summary>
    /// The operation tree of a method declared in the compilation, or null for a method without a body in it.
    /// </summary>
    public static IOperation? MethodBody(Compilation compilation, IMethodSymbol method,
        CancellationToken cancellationToken)
    {
        var syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
        // The semantic model must come from the node's own tree: a method declared in another file has no operation
        // in the model of the caller's tree.
        return syntax is null
            ? null
            : compilation.GetSemanticModel(syntax.SyntaxTree).GetOperation(syntax, cancellationToken);
    }

    /// <summary>
    /// Strips the implicit conversions the compiler wraps around an operand, e.g. to object for a yield return or to
    /// System.Exception for a throw, so that the operand's own kind and type are visible.
    /// </summary>
    public static IOperation WithoutImplicitConversions(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    /// <summary>
    /// The location of the while or do keyword of a loop, where a diagnostic about the loop is reported.
    /// </summary>
    public static Location LoopKeyword(IWhileLoopOperation loop)
    {
        return loop.Syntax switch
        {
            WhileStatementSyntax w => w.WhileKeyword.GetLocation(),
            DoStatementSyntax d => d.DoKeyword.GetLocation(),
            var s => s.GetLocation(),
        };
    }

    /// <summary>
    /// Yields the nodes of a constraint expression such as Throws.TypeOf&lt;T&gt;().With.Message.EqualTo(...) from the
    /// rightmost call to the leftmost member, skipping conversions. An extension-method call
    /// (Is.Not.AllocatingGCMemory()) carries its receiver as the first argument, not as Instance. The walk stops at
    /// any other node (a local, field, parameter, or method result is not followed to its origin: following
    /// initializers needs a semantic model of the declaring file and still misses reassignments).
    /// </summary>
    public static IEnumerable<IOperation> ConstraintChain(IOperation? operation)
    {
        while (operation is not null)
        {
            if (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
                continue;
            }

            yield return operation;
            operation = operation switch
            {
                IInvocationOperation { Instance: { } instance } => instance,
                IInvocationOperation { TargetMethod.IsExtensionMethod: true, Arguments.Length: > 0 } call =>
                    call.Arguments[0].Value,
                IPropertyReferenceOperation property => property.Instance,
                _ => null,
            };
        }
    }

    /// <summary>
    /// Highlights only the member of a chain that the diagnostic is about ("After(...)", "Property(...)", "Length")
    /// rather than the whole chain, which usually starts with unrelated constraints the user must keep.
    /// </summary>
    public static Location MemberNameLocation(SyntaxNode syntax)
    {
        var start = syntax switch
        {
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } => access.Name.SpanStart,
            MemberAccessExpressionSyntax access => access.Name.SpanStart,
            _ => syntax.SpanStart,
        };
        return Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(start, syntax.Span.End));
    }
}
