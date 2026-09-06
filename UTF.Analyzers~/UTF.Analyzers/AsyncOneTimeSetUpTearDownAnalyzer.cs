using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1004: OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncOneTimeSetUpTearDownAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1004";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods",
        messageFormat:
        "'{0}' is not supported on methods that return Task or have the async modifier. Use '{1}' with a coroutine-style test method instead.",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a method marked with OneTimeSetUpAttribute or OneTimeTearDownAttribute whose return type is System.Threading.Tasks.Task (or Task<TResult>), or that has the async modifier. Unity Test Framework runs these methods through NUnit's synchronous command, so a Task-returning method freezes the Editor and an async void method fails.",
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1004.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // The replacement names are literals rather than symbols resolved from the compilation: UnityOneTimeSetUpAttribute
        // and UnityOneTimeTearDownAttribute exist only in Unity Test Framework 1.5.0+, and the rule must still report on 1.4.x.
        var targets = new[]
            {
                (Attribute: context.Compilation.GetTypeByMetadataName("NUnit.Framework.OneTimeSetUpAttribute"),
                    Replacement: "UnityOneTimeSetUpAttribute"),
                (Attribute: context.Compilation.GetTypeByMetadataName("NUnit.Framework.OneTimeTearDownAttribute"),
                    Replacement: "UnityOneTimeTearDownAttribute"),
            }
            .Where(t => t.Attribute is not null)
            .ToImmutableArray();
        var task = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var genericTask = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (targets.IsEmpty || task is null || genericTask is null)
        {
            return;
        }

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            var returnType = method.ReturnType.OriginalDefinition;
            if (!method.IsAsync
                && !SymbolEqualityComparer.Default.Equals(returnType, task)
                && !SymbolEqualityComparer.Default.Equals(returnType, genericTask))
            {
                return;
            }

            foreach (var attribute in method.GetAttributes())
            {
                var target = targets.FirstOrDefault(t =>
                    SymbolEqualityComparer.Default.Equals(attribute.AttributeClass?.OriginalDefinition, t.Attribute));
                if (target.Attribute is null)
                {
                    continue;
                }

                // Reported at the attribute rather than the method name so that each offending attribute is highlighted.
                // ApplicationSyntaxReference is null only for attributes from metadata, which a SymbolAction on source methods never sees.
                var location = attribute.ApplicationSyntaxReference?.GetSyntax(symbolContext.CancellationToken).GetLocation()
                               ?? method.Locations[0];
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, target.Attribute.Name, target.Replacement));
            }
        }, SymbolKind.Method);
    }
}
