using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace UTF.Analyzers.Tests
{
    /// <summary>
    /// Reports NUnit2045 at every Assert member invocation so that a suppressor test has something to suppress.
    /// The real NUnit.Analyzers cannot be used here: its IsAssert check requires the Assert type to live in an
    /// assembly named nunit.framework, and the dummies are compiled into the test assembly.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NUnit2045StubAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new(
            UseAssertMultipleSuppressor.SuppressedDiagnosticId,
            title: "Use Assert.Multiple",
            messageFormat: "Use Assert.Multiple",
            category: "Assertion",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(operationContext =>
            {
                var invocation = (IInvocationOperation)operationContext.Operation;
                if (invocation.TargetMethod.ContainingType.ToDisplayString() == "NUnit.Framework.Assert")
                {
                    operationContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                }
            }, OperationKind.Invocation);
        }
    }
}
