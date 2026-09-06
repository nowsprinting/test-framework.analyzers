using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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
    }
}
