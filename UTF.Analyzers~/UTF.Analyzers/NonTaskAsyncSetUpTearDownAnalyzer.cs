using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF1007: Only Task is supported as an async SetUp and TearDown method return type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonTaskAsyncSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1007";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Only Task is supported as an async SetUp and TearDown method return type",
        messageFormat:
        "'{0}' is not supported as an async SetUp or TearDown method return type: the method is not awaited or the fixture fails. Return 'Task' instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with SetUpAttribute or TearDownAttribute that has the async modifier or an awaitable return type, and whose return type is not System.Threading.Tasks.Task. Unity Test Framework awaits only Task-returning setup and teardown methods: an async void method is invoked but never awaited, and every other return type makes NUnit mark the whole fixture as not runnable.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1007.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var hookAttributes = MethodAttributeAnalysis.TryCreate(context.Compilation,
            MethodAttributeAnalysis.SetUpTearDownAttributes);
        var task = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (hookAttributes is null || task is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!hookAttributes.HasAttribute(method))
            {
                return;
            }

            // Unlike UTF1006, async void is reported here: NUnit.Analyzers has no rule for setup and teardown methods,
            // and Task<TResult> is reported too because UTF1002 covers test methods only. A non-async void method is
            // the common case and is excluded before the member lookup in IsAwaitable.
            var returnType = method.ReturnType;
            if (SymbolEqualityComparer.Default.Equals(returnType, task)
                || (!method.IsAsync
                    && (returnType.SpecialType == SpecialType.System_Void || !AwaitableAnalysis.IsAwaitable(returnType))))
            {
                return;
            }

            var location = MethodAttributeAnalysis.ReturnTypeLocation(method, symbolContext.CancellationToken);
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location,
                returnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }, SymbolKind.Method);
    }
}
