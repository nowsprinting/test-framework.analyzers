using Microsoft.CodeAnalysis;

namespace UTF.Analyzers.Utilities;

/// <summary>
/// Task and Task&lt;TResult&gt; resolved once per compilation, with the "is this method asynchronous" predicate
/// shared by the rules that mirror NUnit's Task detection.
/// </summary>
internal sealed class TaskTypes
{
    private readonly INamedTypeSymbol _task;
    private readonly INamedTypeSymbol _genericTask;

    private TaskTypes(INamedTypeSymbol task, INamedTypeSymbol genericTask)
    {
        _task = task;
        _genericTask = genericTask;
    }

    public static TaskTypes? Resolve(Compilation compilation)
    {
        var task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var genericTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        return task is null || genericTask is null ? null : new TaskTypes(task, genericTask);
    }

    /// <summary>
    /// True for Task and Task&lt;TResult&gt; only. ValueTask and Task-derived types are not matched because NUnit does not treat them as async.
    /// </summary>
    public bool IsTask(ITypeSymbol type)
    {
        var definition = type.OriginalDefinition;
        return SymbolEqualityComparer.Default.Equals(definition, _task) ||
               SymbolEqualityComparer.Default.Equals(definition, _genericTask);
    }

    public bool IsAsyncOrReturnsTask(IMethodSymbol method) => method.IsAsync || IsTask(method.ReturnType);
}
