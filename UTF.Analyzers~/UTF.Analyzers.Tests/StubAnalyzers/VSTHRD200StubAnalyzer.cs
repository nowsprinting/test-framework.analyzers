using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Tests.StubAnalyzers
{
    /// <summary>
    /// Reports VSTHRD200 at the identifier of every ordinary method or local function that returns Task or Task&lt;TResult&gt;
    /// and whose name does not end with "Async", so that a suppressor test has something to suppress. The real
    /// Microsoft.VisualStudio.Threading.Analyzers is not run because its remove-suffix direction fires on the Assert dummy's
    /// ThrowsAsync, CatchAsync, and DoesNotThrowAsync, which are compiled into every fixture; the stub covers the add-suffix
    /// direction only for the same reason.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class VSTHRD200StubAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new(
            AsyncSuffixOnTestMethodSuppressor.SuppressedDiagnosticId,
            title: "Use \"Async\" suffix for async methods",
            messageFormat: "Use \"Async\" suffix in names of methods that return an awaitable type",
            category: "Style",
            // Same default severity as the real VSTHRD200 (Microsoft.VisualStudio.Threading.Analyzers 18.7.23);
            // Roslyn never passes an Error-by-default diagnostic to a suppressor, so the stub must not differ from the real rule here.
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(startContext =>
            {
                var task = startContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var genericTask = startContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
                // One syntax action covers both shapes; symbol actions never visit local functions.
                startContext.RegisterSyntaxNodeAction(nodeContext =>
                {
                    if (nodeContext.SemanticModel.GetDeclaredSymbol(nodeContext.Node, nodeContext.CancellationToken)
                            is not IMethodSymbol method
                        || method.Name.EndsWith("Async", StringComparison.Ordinal))
                    {
                        return;
                    }

                    var returnType = method.ReturnType.OriginalDefinition;
                    if (SymbolEqualityComparer.Default.Equals(returnType, task)
                        || SymbolEqualityComparer.Default.Equals(returnType, genericTask))
                    {
                        // The real rule reports at methodSymbol.Locations[0], i.e. the identifier.
                        nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0]));
                    }
                }, SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement);
            });
        }
    }
}
