using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers;

/// <summary>
/// UTF2002: Async delegates are not supported as the actual value of Throws constraints, Assert.Throws, Assert.Catch, and Assert.DoesNotThrow.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncDelegateInThrowsConstraintAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Async delegates are not supported as the actual value of Throws constraints, Assert.Throws, Assert.Catch, and Assert.DoesNotThrow",
        messageFormat:
        "Async delegates are not supported as the actual value of '{0}'. Test the exception with try/catch in an 'async Task' test method instead.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an async delegate passed as the actual value of a Throws constraint, or passed to Assert.Throws, Assert.Catch, or Assert.DoesNotThrow. NUnit waits for the returned Task synchronously on the calling thread. Unity Test Framework runs tests on the main thread, so the Editor freezes.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2002.md");

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
        var assert = compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        var throws = compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
        var resolveConstraint = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
        var task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var genericTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (assert is null || throws is null || resolveConstraint is null || task is null || genericTask is null)
        {
            return;
        }

        var comparer = SymbolEqualityComparer.Default;
        var that = assert.GetMembers("That").OfType<IMethodSymbol>().ToImmutableHashSet<ISymbol>(comparer);
        var testDelegateAssertions = new[] { "Throws", "Catch", "DoesNotThrow" }
            .SelectMany(name => assert.GetMembers(name).OfType<IMethodSymbol>())
            .ToImmutableHashSet<ISymbol>(comparer);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            var method = invocation.TargetMethod.OriginalDefinition;

            string? receivingApi = null;
            if (that.Contains(method))
            {
                // The constraint is located by parameter type rather than by position so that the message overloads
                // (del, expr, message, args) are matched the same way as the two-argument one.
                var constraint = invocation.Arguments.FirstOrDefault(a =>
                    comparer.Equals(a.Parameter?.Type, resolveConstraint));
                var root = ConstraintRoot(constraint?.Value);
                if (root is null || !comparer.Equals(root.ContainingType, throws))
                {
                    return;
                }

                receivingApi = $"{throws.Name}.{root.Name}";
            }
            else if (testDelegateAssertions.Contains(method))
            {
                receivingApi = $"{assert.Name}.{method.Name}";
            }

            if (receivingApi is null)
            {
                return;
            }

            // The delegate is the argument whose parameter is a delegate type; Assert.Throws(Type, TestDelegate) has it second.
            var argument = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Type.TypeKind == TypeKind.Delegate);
            if (argument is null || !IsAsyncDelegate(argument.Value, task, genericTask))
            {
                return;
            }

            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(), receivingApi));
        }, OperationKind.Invocation);
    }

    /// <summary>
    /// Walks a constraint expression such as Throws.TypeOf&lt;T&gt;().With.Message.EqualTo(...) back to its leftmost member.
    /// Returns null when the expression does not start with a static member access (e.g., a constraint held in a variable).
    /// </summary>
    private static ISymbol? ConstraintRoot(IOperation? operation)
    {
        ISymbol? root = null;
        while (operation is not null)
        {
            switch (operation)
            {
                case IConversionOperation conversion:
                    operation = conversion.Operand;
                    break;
                case IInvocationOperation call:
                    root = call.TargetMethod;
                    operation = call.Instance;
                    break;
                case IPropertyReferenceOperation property:
                    root = property.Property;
                    operation = property.Instance;
                    break;
                default:
                    return null;
            }
        }

        return root;
    }

    /// <summary>
    /// Mirrors NUnit's AsyncInvocationRegion.IsAsyncOperation: the delegate returns Task or Task&lt;TResult&gt;, or its
    /// method is an async method (which is the async void case for TestDelegate).
    /// </summary>
    private static bool IsAsyncDelegate(IOperation value, INamedTypeSymbol task, INamedTypeSymbol genericTask)
    {
        if (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }

        // A Task passed by value (Assert.That(FooAsync(), Throws.X)) is not a delegate creation and is left to NUnit2044.
        var method = (value as IDelegateCreationOperation)?.Target switch
        {
            IAnonymousFunctionOperation lambda => lambda.Symbol,
            IMethodReferenceOperation reference => reference.Method,
            _ => null,
        };
        if (method is null)
        {
            return false;
        }

        var comparer = SymbolEqualityComparer.Default;
        var returnType = method.ReturnType.OriginalDefinition;
        return method.IsAsync || comparer.Equals(returnType, task) || comparer.Equals(returnType, genericTask);
    }
}
