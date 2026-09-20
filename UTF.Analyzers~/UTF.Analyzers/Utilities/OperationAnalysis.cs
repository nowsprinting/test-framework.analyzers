using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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
}
