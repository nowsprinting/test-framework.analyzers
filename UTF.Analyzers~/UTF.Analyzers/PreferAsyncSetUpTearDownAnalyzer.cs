using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4007: Async Task SetUp and TearDown methods are recommended over coroutine-style methods with the UnitySetUp and
/// UnityTearDown attributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferAsyncSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4007";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Async Task SetUp and TearDown methods are recommended over coroutine-style methods with the UnitySetUp and UnityTearDown attributes",
        messageFormat:
        "'{0}' is a coroutine-style method with the '{1}' attribute that yields only Unity yield instructions: it cannot catch an exception across a wait. Use an 'async Task' method with the '{2}' attribute instead.",
        category: "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Detects a coroutine-style setup or teardown method (a method marked with UnitySetUpAttribute or UnityTearDownAttribute whose return type is IEnumerator) whose every yield return yields null or a Unity yield instruction, so that the method can be written as an async Task method with SetUpAttribute or TearDownAttribute. A method that yields anything else, e.g. an Edit Mode instruction such as EnterPlayMode or an IEnumerator returned by the code under test, is not reported.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4007.md");

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
        var walk = UnityYieldWalk.TryCreate(compilation);
        var unitySetUp = compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnitySetUpAttribute");
        var unityTearDown = compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityTearDownAttribute");
        if (walk is null || unitySetUp is null || unityTearDown is null)
        {
            return;
        }

        var hooks = new[] { unitySetUp, unityTearDown };
        context.RegisterSymbolAction(symbolContext =>
        {
            var method = (IMethodSymbol)symbolContext.Symbol;
            // Cheapest check first: the return type rejects almost every method with one comparison, the attribute
            // lookup enumerates the attributes of the method and of every method it overrides, and the body walk
            // is left for the few UnitySetUp and UnityTearDown methods.
            if (!walk.IsCoroutine(method)
                || UnityHookMethodAnalysis.FindAttribute(method, hooks) is not { } hook
                || !walk.IsConvertibleCoroutine(method, symbolContext.CancellationToken))
            {
                return;
            }

            // The replacement is a literal rather than a symbol resolved from the compilation: NUnit's SetUp and
            // TearDown attributes exist wherever the Unity ones do, so there is nothing to resolve.
            var replacement = SymbolEqualityComparer.Default.Equals(hook, unitySetUp)
                ? "SetUpAttribute"
                : "TearDownAttribute";
            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule,
                UnityYieldWalk.SignatureLocation(method, symbolContext.CancellationToken), method.Name, hook.Name,
                replacement));
        }, SymbolKind.Method);
    }
}
