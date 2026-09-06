using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Reports <paramref name="rule"/> at every attribute of the given types applied to a method
/// whose return type is exactly <see cref="System.Collections.IEnumerator"/> (a coroutine-style test method).
/// </summary>
internal static class CoroutineAttributeAnalysis
{
    public static void Register(CompilationStartAnalysisContext context, DiagnosticDescriptor rule, params string[] attributeMetadataNames)
    {
        var targets = attributeMetadataNames
            .Select(context.Compilation.GetTypeByMetadataName)
            .Where(t => t is not null)
            .ToImmutableArray();
        if (targets.IsEmpty)
        {
            return;
        }

        // GetSpecialType is used instead of GetTypeByMetadataName: it never returns null, and matching the exact
        // non-generic IEnumerator symbol excludes IEnumerator<T> without extra checks.
        var enumerator = context.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);

        context.RegisterSymbolAction(symbolContext =>
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();
            var method = (IMethodSymbol)symbolContext.Symbol;
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, enumerator))
            {
                return;
            }

            foreach (var attribute in method.GetAttributes())
            {
                if (!targets.Contains(attribute.AttributeClass?.OriginalDefinition, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                // Reported at the attribute rather than the method name so that each offending attribute is highlighted.
                // ApplicationSyntaxReference is null only for attributes from metadata, which a SymbolAction on source methods never sees.
                var location = attribute.ApplicationSyntaxReference?.GetSyntax(symbolContext.CancellationToken).GetLocation()
                               ?? method.Locations[0];
                symbolContext.ReportDiagnostic(Diagnostic.Create(rule, location, attribute.AttributeClass!.Name));
            }
        }, SymbolKind.Method);
    }
}
