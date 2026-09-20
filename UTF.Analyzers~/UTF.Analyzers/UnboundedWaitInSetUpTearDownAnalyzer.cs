using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF4002: Waits for a condition in UnitySetUp, UnityTearDown, UnityOneTimeSetUp, UnityOneTimeTearDown, SetUp, and
/// TearDown methods must have a time limit.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnboundedWaitInSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF4002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Waits for a condition in UnitySetUp, UnityTearDown, UnityOneTimeSetUp, UnityOneTimeTearDown, SetUp, and TearDown methods must have a time limit",
        messageFormat:
        "'{0}' waits for a condition without a time limit in a method marked with '{1}': the test run hangs when the condition never holds, and a Timeout attribute does not interrupt it. Give the wait a time limit of its own, e.g. a WaitUntil with a TimeSpan timeout or a for loop.",
        category: "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a setup or teardown method that waits for a condition with no time limit of its own: a while or do loop that yields or awaits in its body, or a call to WaitUntil, WaitWhile, or the UniTask.WaitUntil family that is not bounded by a timeout, directly in the method or in helpers, lambdas, and local functions up to two levels deep. Unity Test Framework checks the Timeout attribute and its 180-second default only while the test method body runs, so when the condition never holds the test run hangs.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF4002.md");

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
        var task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (task is null)
        {
            return;
        }

        // Each hook attribute is paired with the only return type Unity Test Framework runs as a coroutine for it:
        // a Task-returning UnitySetUp is never collected, and an async void SetUp returns at its first await.
        var enumerator = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);
        var unityHooks = new[]
        {
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnitySetUpAttribute"),
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityTearDownAttribute"),
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeSetUpAttribute"),
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.UnityOneTimeTearDownAttribute")
        };
        var nunitHooks = new[]
        {
            compilation.GetTypeByMetadataName("NUnit.Framework.SetUpAttribute"),
            compilation.GetTypeByMetadataName("NUnit.Framework.TearDownAttribute")
        };

        var analysis = new WaitAnalysis(compilation);
        context.RegisterSymbolAction(symbolContext =>
        {
            var method = (IMethodSymbol)symbolContext.Symbol;
            var returnType = method.ReturnType.OriginalDefinition;
            var hooks = SymbolEqualityComparer.Default.Equals(returnType, enumerator) ? unityHooks
                : SymbolEqualityComparer.Default.Equals(returnType, task) ? nunitHooks
                : null;
            if (hooks is null || UnityHookMethodAnalysis.FindAttribute(method, hooks) is not { } hook)
            {
                return;
            }

            foreach (var (location, name) in analysis.Waits(method, symbolContext.CancellationToken))
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, name, hook.Name));
            }
        }, SymbolKind.Method);
    }
}
