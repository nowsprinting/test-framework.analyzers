using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

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
        messageFormat:
        "'{0}' is not supported as a test method return type: the Editor freezes. Return 'Task' and assert the value inside the test method instead of using ExpectedResult.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method whose return type is System.Threading.Tasks.Task<TResult>. Unity Test Framework runs only the non-generic Task as an async test; a Task<TResult> method with ExpectedResult is executed synchronously and freezes the Editor.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1002.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var testAttributes = MethodAttributeAnalysis.TryCreate(context.Compilation, MethodAttributeAnalysis.TestAttributes);
        var genericTask = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (testAttributes is null || genericTask is null)
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

            if (!testAttributes.HasAttribute(method))
            {
                return;
            }

            var location = MethodAttributeAnalysis.ReturnTypeLocation(method, symbolContext.CancellationToken);
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location,
                method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }, SymbolKind.Method);
    }
}
