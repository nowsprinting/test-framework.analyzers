using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF2004: Only TestDelegate is supported as the actual value of the AllocatingGCMemory constraint.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonTestDelegateInAllocatingGCMemoryAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2004";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only TestDelegate is supported as the actual value of the AllocatingGCMemory constraint",
        messageFormat:
        "Only TestDelegate is supported as the actual value of the AllocatingGCMemory constraint, but the actual value is '{0}'. Use a lambda with a block body that returns nothing, or a method group of a void method.",
        category: "Assertion",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects an actual value passed to Assert.That or Assume.That together with the AllocatingGCMemory constraint that does not bind to the TestDelegate parameter: a lambda or method group that returns a value, or a variable of a delegate type other than TestDelegate. AllocatingGCMemoryConstraint throws ArgumentException at runtime for these when the constraint is negated or otherwise wrapped.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF2004.md");

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
        var assume = compilation.GetTypeByMetadataName("NUnit.Framework.Assume");
        var testDelegate = compilation.GetTypeByMetadataName("NUnit.Framework.TestDelegate");
        var resolveConstraint = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
        var allocating =
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.Constraints.AllocatingGCMemoryConstraint");
        if (assert is null || assume is null || testDelegate is null || resolveConstraint is null || allocating is null)
        {
            return;
        }

        var that = assert.GetMembers("That").Concat(assume.GetMembers("That"))
            .ToImmutableHashSet(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            if (!that.Contains(invocation.TargetMethod.OriginalDefinition))
            {
                return;
            }

            // The actual value is the argument bound to the first parameter whatever its type; the constraint is the
            // argument bound to IResolveConstraint, so the message overloads (actual, expr, message, args) match too.
            IArgumentOperation? actual = null;
            IOperation? constraint = null;
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter?.Ordinal == 0)
                {
                    actual = argument;
                }
                else if (SymbolEqualityComparer.Default.Equals(argument.Parameter?.Type, resolveConstraint))
                {
                    constraint = argument.Value;
                }
            }

            if (actual?.Parameter is null || constraint is null ||
                SymbolEqualityComparer.Default.Equals(actual.Parameter.Type, testDelegate) ||
                !ChainCreates(constraint, allocating))
            {
                return;
            }

            // An async delegate under this constraint freezes the Editor before the TestDelegate check matters;
            // UTF2003 owns it, and the shared classifier keeps the two rules from double-reporting.
            if (AsyncDelegateAnalysis.IsAsyncDelegate(actual))
            {
                return;
            }

            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, actual.Syntax.GetLocation(),
                actual.Parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }, OperationKind.Invocation);
    }

    /// <summary>
    /// Walks the receiver chain of a constraint expression such as Is.Not.AllocatingGCMemory().After(10) and tells
    /// whether any call in it returns AllocatingGCMemoryConstraint or any object creation is of that type.
    /// The static type of the whole argument is not enough: wrappers such as After turn it into DelayedConstraint.
    /// A constraint held in a local, field, parameter, or method result is not followed to its origin, for the same
    /// reason as in AsyncDelegateAnalysis.ThrowsRoot.
    /// </summary>
    private static bool ChainCreates(IOperation? operation, INamedTypeSymbol constraintType)
    {
        while (operation is not null)
        {
            switch (operation)
            {
                case IConversionOperation conversion:
                    operation = conversion.Operand;
                    break;
                case IInvocationOperation call:
                    if (SymbolEqualityComparer.Default.Equals(call.TargetMethod.ReturnType.OriginalDefinition,
                            constraintType))
                    {
                        return true;
                    }

                    // An extension method call (Is.Not.AllocatingGCMemory()) carries its receiver as the first
                    // argument, not as Instance.
                    operation = call.Instance ?? call.Arguments.FirstOrDefault()?.Value;
                    break;
                case IPropertyReferenceOperation property:
                    operation = property.Instance;
                    break;
                case IObjectCreationOperation creation:
                    return SymbolEqualityComparer.Default.Equals(creation.Type?.OriginalDefinition, constraintType);
                default:
                    return false;
            }
        }

        return false;
    }
}
