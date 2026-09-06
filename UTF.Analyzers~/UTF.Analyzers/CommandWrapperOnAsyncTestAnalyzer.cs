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
        helpLinkUri: "https://nowsprinting.github.io/test-framework.analyzers/Documentation~/rules/UTF1005.md");

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
        var testBuilders = new[]
            {
                compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ITestBuilder"),
                compilation.GetTypeByMetadataName("NUnit.Framework.Interfaces.ISimpleTestBuilder"),
            }
            .Where(t => t is not null)
            .ToImmutableArray();
        if (commandWrapper is null || task is null || testBuilders.IsEmpty)
        {
            return;
        }

        // Unity Test Framework substitutes the commands produced by these attributes by exact type name,
        // so the exemption is by exact type as well; a derived attribute is reported.
        var exempt = new[]
            {
                compilation.GetTypeByMetadataName("NUnit.Framework.RepeatAttribute"),
                compilation.GetTypeByMetadataName("NUnit.Framework.RetryAttribute"),
                compilation.GetTypeByMetadataName("NUnit.Framework.MaxTimeAttribute"),
                compilation.GetTypeByMetadataName("UnityEngine.TestTools.ParametrizedIgnoreAttribute"),
            }
            .Where(t => t is not null)
            .ToImmutableArray();
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

            var attributes = method.GetAttributes();
            if (!attributes.Any(a => Implements(a.AttributeClass, testBuilders)))
            {
                return;
            }

            foreach (var attribute in attributes)
            {
                var attributeClass = attribute.AttributeClass;
                if (attributeClass is null
                    || !attributeClass.AllInterfaces.Contains(commandWrapper, SymbolEqualityComparer.Default)
                    || exempt.Contains(attributeClass.OriginalDefinition, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                // Reported at the attribute rather than the method name so that each offending attribute is highlighted.
                var location = attribute.ApplicationSyntaxReference?.GetSyntax(symbolContext.CancellationToken).GetLocation()
                               ?? method.Locations[0];
                symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, attributeClass.Name));
            }
        }, SymbolKind.Method);
    }

    private static bool Implements(INamedTypeSymbol? type, ImmutableArray<INamedTypeSymbol?> interfaces)
    {
        return type is not null
               && type.AllInterfaces.Any(i => interfaces.Contains(i.OriginalDefinition, SymbolEqualityComparer.Default));
    }
}
