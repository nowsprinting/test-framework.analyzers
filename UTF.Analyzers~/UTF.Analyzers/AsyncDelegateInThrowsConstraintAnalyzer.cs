using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        "Async delegates are not supported as the actual value of '{0}'. Test the exception with try/catch in an async test method instead.",
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
        if (assert is null || throws is null || resolveConstraint is null)
        {
            return;
        }

        // ThrowsNothingConstraint may be absent from an older NUnit; the set simply omits it.
        var throwsConstraintTypes = new[]
            {
                "NUnit.Framework.Constraints.ThrowsConstraint", "NUnit.Framework.Constraints.ThrowsNothingConstraint",
            }
            .Select(context.Compilation.GetTypeByMetadataName)
            .Where(t => t is not null)
            .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);

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
            if (argument is null || !IsAsyncDelegate(argument))
            {
                return;
            }

            // The name is built from the symbol rather than the syntax so that a call through "using static" still reads "Throws.X".
            var receivingApi = $"{assert.Name}.{method.Name}";
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

                var root = ThrowsRoot(constraint, operationContext.Compilation, throws, throwsConstraintTypes,
                    depth: 0, operationContext.CancellationToken);
                if (root is null)
                {
                    return;
                }

                receivingApi = root;
            }

            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(), receivingApi));
        }, OperationKind.Invocation);
    }

    /// <summary>
    /// Walks a constraint expression such as Throws.TypeOf&lt;T&gt;().With.Message.EqualTo(...) back to its leftmost member
    /// and returns its display name ("Throws.TypeOf", "ThrowsConstraint") when that member belongs to Throws or is a
    /// ThrowsConstraint construction, or null otherwise.
    /// A local or field reference is followed to its initializer. Reassignments are not tracked: full data-flow
    /// analysis is not worth its cost for test code, and an untracked Throws constraint falls through to UTF2003
    /// rather than going unreported.
    /// </summary>
    private static string? ThrowsRoot(IOperation? operation, Compilation compilation, INamedTypeSymbol throws,
        ImmutableHashSet<ISymbol> throwsConstraintTypes, int depth, CancellationToken cancellationToken)
    {
        // Fields can initialize each other in a cycle; the bound keeps the walk finite.
        if (depth > 8)
        {
            return null;
        }

        ISymbol? root = null;
        while (operation is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                case IObjectCreationOperation creation:
                    return creation.Type is not null && throwsConstraintTypes.Contains(creation.Type.OriginalDefinition)
                        ? creation.Type.Name
                        : null;
                case ILocalReferenceOperation local:
                    return ThrowsRoot(Initializer(local.Local, compilation, cancellationToken), compilation, throws,
                        throwsConstraintTypes, depth + 1, cancellationToken);
                case IFieldReferenceOperation field:
                    return ThrowsRoot(Initializer(field.Field, compilation, cancellationToken), compilation, throws,
                        throwsConstraintTypes, depth + 1, cancellationToken);
                default:
                    return null;
            }
        }

        return root is not null && SymbolEqualityComparer.Default.Equals(root.ContainingType, throws)
            ? $"{throws.Name}.{root.Name}"
            : null;
    }

    private static IOperation? Initializer(ISymbol symbol, Compilation compilation, CancellationToken cancellationToken)
    {
        // Locals and fields share VariableDeclaratorSyntax; a symbol from metadata has no syntax reference.
        var declarator = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken)
            as VariableDeclaratorSyntax;
        var value = declarator?.Initializer?.Value;
        return value is null
            ? null
            : compilation.GetSemanticModel(value.SyntaxTree).GetOperation(value, cancellationToken);
    }

    /// <summary>
    /// The delegate counts as async when its return type is awaitable or its creation target has the async modifier.
    /// NUnit's own check (return type name starts with "System.Threading.Tasks.Task", or AsyncStateMachineAttribute) is
    /// deliberately not mirrored: a non-async lambda returning ValueTask, UniTask, or Awaitable slips past that check,
    /// so NUnit neither waits for it nor observes the exception thrown after the first await, and the test fails or
    /// passes for the wrong reason. The fix is the same try/catch, so the rule reports every awaitable.
    /// The return type is read from the bound delegate parameter (ActualValueDelegate&lt;Task&gt;) rather than from the
    /// lambda, so a delegate held in a variable is covered too; the async modifier must come from the creation target
    /// because it is the only trace of an async void lambda on a TestDelegate.
    /// </summary>
    private static bool IsAsyncDelegate(IArgumentOperation argument)
    {
        var invoke = ((INamedTypeSymbol)argument.Parameter!.Type).DelegateInvokeMethod;
        if (invoke is not null && IsAwaitable(invoke.ReturnType))
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

    /// <summary>
    /// The awaitable pattern is matched by member name because the language defines it that way; there is no symbol
    /// to compare against. Extension-method GetAwaiter is not resolved, which is a known limitation of the rule.
    /// </summary>
    private static bool IsAwaitable(ITypeSymbol type) =>
        type.GetMembers("GetAwaiter").OfType<IMethodSymbol>()
            .Any(m => !m.IsStatic && m.Parameters.IsEmpty && m.TypeParameters.IsEmpty);
}
