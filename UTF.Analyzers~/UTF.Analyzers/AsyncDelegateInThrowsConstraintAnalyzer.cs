using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

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
        var assert = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        var throws = context.Compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
        var resolveConstraint =
            context.Compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
        var taskTypes = TaskTypes.Resolve(context.Compilation);
        if (assert is null || throws is null || resolveConstraint is null || taskTypes is null)
        {
            return;
        }

        var that = assert.GetMembers("That").OfType<IMethodSymbol>()
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);
        var testDelegateAssertions = new[] { "Throws", "Catch", "DoesNotThrow" }
            .SelectMany(name => assert.GetMembers(name).OfType<IMethodSymbol>())
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            var method = invocation.TargetMethod.OriginalDefinition;
            var isThat = that.Contains(method);
            if (!isThat && !testDelegateAssertions.Contains(method))
            {
                return;
            }

            // The delegate is checked before the constraint because it is the cheaper and more selective filter:
            // most Assert.That calls take a plain value, and most Assert.Throws calls take a synchronous lambda.
            // Arguments are located by parameter type rather than by position so that the message overloads
            // (del, expr, message, args) and Assert.Throws(Type, TestDelegate) are matched the same way.
            var argument = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Type.TypeKind == TypeKind.Delegate);
            if (argument is null || !IsAsyncDelegate(argument, taskTypes))
            {
                return;
            }

            ISymbol receivingApi = method;
            if (isThat)
            {
                IOperation? constraint = null;
                foreach (var a in invocation.Arguments)
                {
                    if (SymbolEqualityComparer.Default.Equals(a.Parameter?.Type, resolveConstraint))
                    {
                        constraint = a.Value;
                        break;
                    }
                }

                var root = ConstraintRoot(constraint);
                if (root is null || !SymbolEqualityComparer.Default.Equals(root.ContainingType, throws))
                {
                    return;
                }

                receivingApi = root;
            }

            // The name is built from the symbol rather than the syntax so that a call through "using static" still reads "Throws.X".
            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(),
                $"{receivingApi.ContainingType.Name}.{receivingApi.Name}"));
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
    /// Mirrors NUnit's AsyncInvocationRegion.IsAsyncOperation. The return type is read from the bound delegate parameter
    /// (ActualValueDelegate&lt;Task&gt;) rather than from the lambda, so a delegate held in a variable is covered too;
    /// the async modifier must come from the creation target because it is the only trace of an async void lambda on a TestDelegate.
    /// </summary>
    private static bool IsAsyncDelegate(IArgumentOperation argument, TaskTypes taskTypes)
    {
        var invoke = ((INamedTypeSymbol)argument.Parameter!.Type).DelegateInvokeMethod;
        if (invoke is not null && taskTypes.IsTask(invoke.ReturnType))
        {
            return true;
        }

        var value = argument.Value is IConversionOperation conversion ? conversion.Operand : argument.Value;
        return (value as IDelegateCreationOperation)?.Target switch
        {
            IAnonymousFunctionOperation lambda => lambda.Symbol.IsAsync,
            IMethodReferenceOperation reference => reference.Method.IsAsync,
            _ => false,
        };
    }
}
