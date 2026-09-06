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
        var analysis = AsyncDelegateAnalysis.TryCreate(context.Compilation);
        var testDelegate = context.Compilation.GetTypeByMetadataName("NUnit.Framework.TestDelegate");
        var allocating = context.Compilation.GetTypeByMetadataName(
            "UnityEngine.TestTools.Constraints.AllocatingGCMemoryConstraint");
        if (analysis is null || testDelegate is null || allocating is null)
        {
            return;
        }

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)operationContext.Operation;
            if (!analysis.IsThat(invocation.TargetMethod))
            {
                return;
            }

            // The actual value is the argument bound to the first parameter whatever its type (TestDelegate,
            // ActualValueDelegate<TActual>, or TActual), so it is located by ordinal rather than by parameter type.
            var actual = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == 0);
            if (actual is null || SymbolEqualityComparer.Default.Equals(actual.Parameter!.Type, testDelegate) ||
                !ChainCreates(analysis.ConstraintArgument(invocation), allocating))
            {
                return;
            }

            // An async delegate under this constraint is owned by UTF2003 (or UTF2002 through a Throws chain), so the
            // shared classifier decides the exclusion instead of this rule re-deriving "is async" on its own.
            if (analysis.Classify(invocation, out _, out _) != AsyncDelegateAnalysis.Owner.None)
            {
                return;
            }

            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, actual.Syntax.GetLocation(),
                actual.Parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }, OperationKind.Invocation);
    }

    /// <summary>
    /// The static type of the constraint argument is not enough: wrappers such as After turn it into DelayedConstraint,
    /// so every call and object creation in the chain is checked. A local is followed to its initializer because
    /// "var constraint = Is.Not.AllocatingGCMemory();" is a natural way to write the assertion; the shared walker
    /// does not do this because a variable-held Throws constraint is deliberately left to UTF2003.
    /// ponytail: reassignment after the initializer is not seen, add flow analysis if it ever matters.
    /// </summary>
    private static bool ChainCreates(IOperation? constraint, INamedTypeSymbol constraintType)
    {
        foreach (var node in AsyncDelegateAnalysis.ConstraintChain(constraint))
        {
            switch (node)
            {
                case IInvocationOperation call when SymbolEqualityComparer.Default.Equals(
                    call.TargetMethod.ReturnType.OriginalDefinition, constraintType):
                case IObjectCreationOperation creation when SymbolEqualityComparer.Default.Equals(
                    creation.Type?.OriginalDefinition, constraintType):
                    return true;
                case ILocalReferenceOperation local:
                    return ChainCreates(Initializer(local), constraintType);
            }
        }

        return false;
    }

    private static IOperation? Initializer(ILocalReferenceOperation reference)
    {
        var root = (IOperation)reference;
        while (root.Parent is not null)
        {
            root = root.Parent;
        }

        return root.Descendants().OfType<IVariableDeclaratorOperation>()
            .FirstOrDefault(d => SymbolEqualityComparer.Default.Equals(d.Symbol, reference.Local))
            ?.Initializer?.Value;
    }
}
