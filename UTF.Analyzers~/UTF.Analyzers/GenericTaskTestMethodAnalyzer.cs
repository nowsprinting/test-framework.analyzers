using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1002: Task&lt;TResult&gt; is not supported as a test method return type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GenericTaskTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Task<TResult> is not supported as a test method return type",
        messageFormat: "Task<TResult> is not supported as a test method return type. Return `Task` and assert the value inside the test method instead of using ExpectedResult.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Detects a test method whose return type is System.Threading.Tasks.Task<TResult>. Unity Test Framework runs only the non-generic Task as an async test; a Task<TResult> method with ExpectedResult is executed synchronously and freezes the Editor.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1002.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var testAttributes = new[]
            {
                context.Compilation.GetTypeByMetadataName("NUnit.Framework.TestAttribute"),
                context.Compilation.GetTypeByMetadataName("NUnit.Framework.TestCaseAttribute"),
                context.Compilation.GetTypeByMetadataName("NUnit.Framework.TestCaseSourceAttribute"),
            }
            .Where(t => t is not null)
            .ToImmutableArray();
        var genericTask = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (testAttributes.IsEmpty || genericTask is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType.OriginalDefinition, genericTask))
            {
                return;
            }

            if (!method.GetAttributes().Any(a => testAttributes.Contains(a.AttributeClass?.OriginalDefinition, SymbolEqualityComparer.Default)))
            {
                return;
            }

            // The defect is the return type, not any single attribute, so one diagnostic is reported at the return type
            // even when the method carries several test attributes. A partial method has more than one declaration; the first is enough.
            var location = method.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax(symbolContext.CancellationToken))
                .OfType<MethodDeclarationSyntax>()
                .Select(m => m.ReturnType.GetLocation())
                .FirstOrDefault() ?? method.Locations[0];
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }, SymbolKind.Method);
    }
}
