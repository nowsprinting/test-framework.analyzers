using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers;

/// <summary>
/// UTF1005: Attributes implementing ICommandWrapper are not supported on async and coroutine-style test methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandWrapperOnAsyncTestAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF1005";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Attributes implementing ICommandWrapper are not supported on async and coroutine-style test methods",
        messageFormat: "'{0}' is not supported on async and coroutine-style test methods",
        category: "Structure",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Detects a test method whose return type is System.Threading.Tasks.Task or System.Collections.IEnumerator and that is marked with an attribute implementing NUnit.Framework.Interfaces.ICommandWrapper. Unity Test Framework runs such methods through its own coroutine-driven commands, which a user-defined wrapper cannot drive; the test body never runs.",
        helpLinkUri: "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF1005.md");

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
        var commandWrapper = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ICommandWrapper");
        var task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        // Test methods are recognized through the builder interfaces rather than a list of attribute names,
        // so user-defined test builders are covered the same way Unity Test Framework discovers them.
        var testBuilder = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ITestBuilder");
        var simpleTestBuilder = compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ISimpleTestBuilder");
        if (commandWrapper is null || task is null || testBuilder is null || simpleTestBuilder is null)
        {
            return;
        }

        // Unity Test Framework substitutes the commands produced by these attributes by exact type name,
        // so the exemption is by exact type as well; a derived attribute is reported.
        // A null entry (type not referenced) never equals an attribute class, so no filtering is needed.
        var exempt = new[]
        {
            compilation.GetTypeByMetadataName("NUnit.Framework.RepeatAttribute"),
            compilation.GetTypeByMetadataName("NUnit.Framework.RetryAttribute"),
            compilation.GetTypeByMetadataName("NUnit.Framework.MaxTimeAttribute"),
            compilation.GetTypeByMetadataName("UnityEngine.TestTools.ParametrizedIgnoreAttribute"),
        };
        var enumerator = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, task)
                && !SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            {
                return;
            }

            // Non-test methods such as [SetUp] are not reported: Unity Test Framework reads wrapper attributes only from
            // the test method (TestCommandBuilder), so a wrapper on any other method is inert rather than harmful.
            var attributes = method.GetAttributes();
            if (!attributes.Any(a => a.AttributeClass is { } c
                                     && (c.AllInterfaces.Contains(testBuilder, SymbolEqualityComparer.Default)
                                         || c.AllInterfaces.Contains(simpleTestBuilder,
                                             SymbolEqualityComparer.Default))))
            {
                return;
            }

            foreach (var attribute in attributes)
            {
                var attributeClass = attribute.AttributeClass;
                if (attributeClass is null
                    || !attributeClass.AllInterfaces.Contains(commandWrapper, SymbolEqualityComparer.Default)
                    || exempt.Contains(attributeClass, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                // Reported at the attribute rather than the method name so that each offending attribute is highlighted.
                var location = attribute.ApplicationSyntaxReference?.GetSyntax(symbolContext.CancellationToken)
                                   .GetLocation()
                               ?? method.Locations[0];
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, attributeClass.Name));
            }
        }, SymbolKind.Method);
    }
}
