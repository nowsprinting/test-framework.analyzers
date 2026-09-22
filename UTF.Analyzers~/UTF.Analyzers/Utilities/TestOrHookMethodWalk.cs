using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Walks every test and hook method of a compilation with a <see cref="DepthBoundedWalker"/> and reports each wait
/// it finds, once per location. Shared by UTF4003 and UTF4004, which report at the wait itself: a helper reached
/// from several methods is walked once per caller, and the set keeps the first report only.
/// </summary>
internal sealed class TestOrHookMethodWalk
{
    private readonly Compilation _compilation;
    private readonly TestMethodAnalysis _testMethods;
    private readonly INamedTypeSymbol?[] _hooks;
    private readonly DiagnosticDescriptor _rule;
    private readonly Func<CancellationToken, DepthBoundedWalker> _createWalker;
    private readonly ConcurrentDictionary<Location, bool> _reported = new();

    private TestOrHookMethodWalk(Compilation compilation, TestMethodAnalysis testMethods, DiagnosticDescriptor rule,
        Func<CancellationToken, DepthBoundedWalker> createWalker)
    {
        _compilation = compilation;
        _testMethods = testMethods;
        _hooks = UnityHookMethodAnalysis.ResolveAllHookAttributes(compilation);
        _rule = rule;
        _createWalker = createWalker;
    }

    /// <summary>
    /// Registers the walk on <paramref name="context"/>; does nothing when NUnit is absent from the compilation.
    /// </summary>
    public static void Register(CompilationStartAnalysisContext context, DiagnosticDescriptor rule,
        Func<CancellationToken, DepthBoundedWalker> createWalker)
    {
        if (TestMethodAnalysis.TryCreate(context.Compilation) is { } testMethods)
        {
            var walk = new TestOrHookMethodWalk(context.Compilation, testMethods, rule, createWalker);
            context.RegisterSymbolAction(walk.AnalyzeMethod, SymbolKind.Method);
        }
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (!_testMethods.IsTestMethod(method) && !UnityHookMethodAnalysis.HasAnyAttribute(method, _hooks))
        {
            return;
        }

        if (OperationAnalysis.MethodBody(_compilation, method, context.CancellationToken) is not { } body)
        {
            return;
        }

        var walker = _createWalker(context.CancellationToken);
        walker.Visit(body, 0);
        foreach (var (location, name) in walker.Found)
        {
            if (_reported.TryAdd(location, true))
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, location, name));
            }
        }
    }
}
