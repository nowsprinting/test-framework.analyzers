using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF1006: Only Task is supported as an async test method return type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonTaskAsyncTestMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1006";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only Task is supported as an async test method return type",
        messageFormat:
        "'{0}' is not supported as an async test method return type: the test fails without running. Return 'Task' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method that has the async modifier or an awaitable return type, and whose return type is not System.Threading.Tasks.Task. Unity Test Framework runs only the non-generic Task as an async test; every other return type, such as UniTask, UniTaskVoid, Awaitable, or ValueTask, is run through NUnit's synchronous command and fails before the test body completes.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1006.md");

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
        var task = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var genericTask = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (testAttributes.IsEmpty || task is null || genericTask is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            var returnType = method.ReturnType;
            // void is left to NUnit1012, Task is the supported type, and Task<TResult> is UTF1002's; these are excluded
            // before the awaitable check because Task itself satisfies it.
            if (returnType.SpecialType == SpecialType.System_Void
                || SymbolEqualityComparer.Default.Equals(returnType, task)
                || SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, genericTask))
            {
                return;
            }

            if (!method.IsAsync && !AwaitableAnalysis.IsAwaitable(returnType))
            {
                return;
            }

            if (!method.GetAttributes().Any(a =>
                    testAttributes.Contains(a.AttributeClass?.OriginalDefinition, SymbolEqualityComparer.Default)))
            {
                return;
            }

            // The defect is the return type, not any single attribute, so one diagnostic is reported at the return type
            // even when several test attributes are applied to the method.
            var location =
                (method.DeclaringSyntaxReferences[0].GetSyntax(symbolContext.CancellationToken) as
                    MethodDeclarationSyntax)
                ?.ReturnType.GetLocation() ?? method.Locations[0];
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location,
                returnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }, SymbolKind.Method);
    }
}
