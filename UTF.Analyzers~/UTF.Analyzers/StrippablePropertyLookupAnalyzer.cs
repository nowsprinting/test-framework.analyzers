using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using UTF.Analyzers.Utilities;

namespace UTF.Analyzers;

/// <summary>
/// UTF2006: Property constraints, Ordered.By, and List.Map(...).Property look up properties that managed code stripping can remove.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StrippablePropertyLookupAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "UTF2006";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title:
        "Property constraints, Ordered.By, and List.Map(...).Property look up properties that managed code stripping can remove",
        messageFormat:
        "'{0}' is looked up by name at run time and managed code stripping can remove it: the test fails in the Player. Read the property directly in the actual value instead.",
        category: "Assertion",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description:
        "Detects a property constraint (Has.Property, With.Property, Has.Length, Has.Count, Has.Message, Has.InnerException), Is.Ordered.By, and List.Map(...).Property whose property is not referenced by NUnit itself. NUnit finds the property with reflection at run time, so the Unity linker sees no reference to it and can remove it from a Player build.",
        helpLinkUri:
        "https://github.com/nowsprinting/test-framework.analyzers/tree/master/Documentation~/rules/UTF2006.md");

    /// <summary>
    /// Property getters of class library types that nunit.framework.dll in com.unity.ext.nunit 2.0.5 calls directly,
    /// taken from its IL. Kept as metadata names rather than read from the referenced nunit.framework.dll: Roslyn exposes
    /// no method bodies of metadata references, and in tests NUnit is a dummy source.
    /// </summary>
    private static readonly string[] NUnitReferencedGetters =
    {
        "System.AggregateException|InnerExceptions",
        "System.AppDomain|CurrentDomain,FriendlyName",
        "System.Array|Length,Rank",
        "System.Collections.DictionaryEntry|Key,Value",
        "System.Collections.Generic.Dictionary`2|Count,Keys",
        "System.Collections.Generic.ICollection`1|Count",
        "System.Collections.Generic.IDictionary`2|Keys",
        "System.Collections.Generic.IEnumerator`1|Current",
        "System.Collections.Generic.KeyValuePair`2|Key,Value",
        "System.Collections.Generic.List`1|Count",
        "System.Collections.Generic.Stack`1|Count",
        "System.Collections.ICollection|Count",
        "System.Collections.IDictionary|Keys,Values",
        "System.Collections.IEnumerator|Current",
        "System.Console|Error,Out",
        "System.DateTime|Now,UtcNow",
        "System.DateTimeOffset|Offset",
        "System.Delegate|Method",
        "System.Diagnostics.Debugger|IsAttached",
        "System.Diagnostics.Process|Id",
        "System.Environment|CurrentDirectory,Is64BitOperatingSystem,MachineName,NewLine,OSVersion,UserDomainName,UserName,Version",
        "System.Exception|InnerException,Message,StackTrace",
        "System.Globalization.CultureInfo|CurrentCulture,CurrentUICulture,InvariantCulture,Name,NumberFormat,TwoLetterISOLanguageName",
        "System.Globalization.NumberFormatInfo|InvariantInfo",
        "System.IO.BinaryReader|BaseStream",
        "System.IO.FileSystemInfo|Attributes,CreationTime,Exists,FullName,LastAccessTime",
        "System.IO.Stream|CanRead,CanSeek,Length,Position",
        "System.IO.TextWriter|Encoding",
        "System.IntPtr|Size",
        "System.Lazy`1|Value",
        "System.Nullable`1|HasValue,Value",
        "System.OperatingSystem|Platform,Version",
        "System.Reflection.Assembly|CodeBase,FullName,Location",
        "System.Reflection.AssemblyName|Version",
        "System.Reflection.FieldInfo|FieldType,IsStatic",
        "System.Reflection.MemberInfo|DeclaringType,Name",
        "System.Reflection.MethodBase|ContainsGenericParameters,IsAbstract,IsFamily,IsGenericMethod,IsGenericMethodDefinition,IsPublic,IsStatic",
        "System.Reflection.MethodInfo|ReturnType",
        "System.Reflection.ParameterInfo|IsOptional,Member,ParameterType",
        "System.Reflection.PropertyInfo|PropertyType",
        "System.String|Length",
        "System.Text.Encoding|Default",
        "System.Text.RegularExpressions.Capture|Value",
        "System.Text.StringBuilder|Length",
        "System.Threading.CountdownEvent|CurrentCount",
        "System.Threading.Thread|CurrentPrincipal,CurrentThread,ManagedThreadId,ThreadState",
        "System.TimeSpan|TotalMilliseconds,TotalSeconds",
        "System.Type|Assembly,BaseType,ContainsGenericParameters,FullName,GenericParameterPosition,HasElementType,IsAbstract,IsArray,IsClass,IsEnum,IsGenericParameter,IsGenericType,IsGenericTypeDefinition,IsSealed,Namespace",
        "System.Version|Build,Major,Minor,Revision",
        "System.Xml.XmlNode|Attributes,ChildNodes,FirstChild,InnerText,Name,NodeType,Value",
    };

    private static readonly (string Property, string ExceptionType)[] ThrowsExceptionProperties =
    {
        ("Exception", "System.Exception"),
        ("ArgumentException", "System.ArgumentException"),
        ("ArgumentNullException", "System.ArgumentNullException"),
        ("InvalidOperationException", "System.InvalidOperationException"),
        ("TargetInvocationException", "System.Reflection.TargetInvocationException"),
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var analysis = Analysis.TryCreate(context.Compilation);
        if (analysis is null)
        {
            return;
        }

        context.RegisterOperationAction(operationContext =>
        {
            operationContext.CancellationToken.ThrowIfCancellationRequested();
            analysis.Analyze((IInvocationOperation)operationContext.Operation, operationContext.ReportDiagnostic);
        }, OperationKind.Invocation);
    }

    private sealed class Analysis
    {
        private readonly INamedTypeSymbol _resolveConstraint;
        private readonly INamedTypeSymbol? _systemType;
        private readonly INamedTypeSymbol? _enumerableOfT;
        private readonly ImmutableHashSet<ISymbol> _that;
        private readonly ImmutableDictionary<ISymbol, string?> _propertySteps;
        private readonly ImmutableHashSet<ISymbol> _collectionOperators;
        private readonly ImmutableHashSet<ISymbol> _typeConstraints;
        private readonly ImmutableDictionary<ISymbol, INamedTypeSymbol> _throwsExceptions;
        private readonly ImmutableHashSet<ISymbol> _conjunctions;
        private readonly ImmutableHashSet<ISymbol> _by;
        private readonly ImmutableHashSet<ISymbol> _listMap;
        private readonly ImmutableHashSet<ISymbol> _listMapperProperty;
        private readonly ImmutableHashSet<ISymbol> _nunitReferenced;

        private Analysis(Compilation compilation, INamedTypeSymbol assert, INamedTypeSymbol has,
            INamedTypeSymbol constraintExpression, INamedTypeSymbol constraint, INamedTypeSymbol resolveConstraint)
        {
            var assume = compilation.GetTypeByMetadataName("NUnit.Framework.Assume");
            var @is = compilation.GetTypeByMetadataName("NUnit.Framework.Is");
            var throws = compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
            var resolvable =
                compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.ResolvableConstraintExpression");
            var ordered = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.CollectionOrderedConstraint");
            var list = compilation.GetTypeByMetadataName("NUnit.Framework.List");
            var listMapper = compilation.GetTypeByMetadataName("NUnit.Framework.ListMapper");
            var expressionTypes = new[] { has, constraintExpression };

            _resolveConstraint = resolveConstraint;
            _systemType = compilation.GetTypeByMetadataName("System.Type");
            _enumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            _that = Members(new[] { assert, assume }, "That");
            // A null name means Property(string), whose name is the constant argument.
            _propertySteps = new[] { "Property", "Length", "Count", "Message", "InnerException" }
                .SelectMany(name => Members(expressionTypes, name)
                    .Select(member => (member,
                        name: string.Equals(name, "Property", System.StringComparison.Ordinal) ? null : name)))
                .ToImmutableDictionary(p => p.member, p => p.name, SymbolEqualityComparer.Default,
                    System.StringComparer.Ordinal);
            _collectionOperators = Members(expressionTypes, "All", "Some", "None", "Exactly");
            _typeConstraints = Members(new[] { @is, throws, constraintExpression }, "TypeOf", "InstanceOf")
                .Where(member => member is IMethodSymbol { IsGenericMethod: true })
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
            _throwsExceptions = ThrowsExceptionProperties
                .Select(p => (member: throws?.GetMembers(p.Property).FirstOrDefault(),
                    type: compilation.GetTypeByMetadataName(p.ExceptionType)))
                .Where(p => p.member is not null && p.type is not null)
                .ToImmutableDictionary(p => p.member!, p => p.type!, SymbolEqualityComparer.Default);
            // Constraint.With is Constraint.And, not the prefix ConstraintExpression.With, so it starts a new operand.
            _conjunctions = Members(new[] { constraint }, "And", "Or", "With")
                .Union(Members(new[] { resolvable }, "And", "Or"));
            _by = Members(new[] { ordered }, "By");
            _listMap = Members(new[] { list }, "Map");
            _listMapperProperty = Members(new[] { listMapper }, "Property");
            _nunitReferenced = NUnitReferencedGetters
                .Select(line => line.Split('|'))
                .SelectMany(parts => compilation.GetTypeByMetadataName(parts[0]) is { } type
                    ? parts[1].Split(',').SelectMany(name => type.GetMembers(name).OfType<IPropertySymbol>())
                    : Enumerable.Empty<IPropertySymbol>())
                .ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);
        }

        public static Analysis? TryCreate(Compilation compilation)
        {
            var assert = compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
            var has = compilation.GetTypeByMetadataName("NUnit.Framework.Has");
            var constraintExpression =
                compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.ConstraintExpression");
            var constraint = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.Constraint");
            var resolveConstraint = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");
            return assert is null || has is null || constraintExpression is null || constraint is null ||
                   resolveConstraint is null
                ? null
                : new Analysis(compilation, assert, has, constraintExpression, constraint, resolveConstraint);
        }

        public void Analyze(IInvocationOperation invocation, System.Action<Diagnostic> report)
        {
            var method = invocation.TargetMethod.OriginalDefinition;
            if (_that.Contains(method))
            {
                AnalyzeThat(invocation, report);
            }
            else if (_listMapperProperty.Contains(method) &&
                     invocation.Instance is IInvocationOperation map &&
                     _listMap.Contains(map.TargetMethod.OriginalDefinition) &&
                     map.Arguments.Length == 1)
            {
                var collection = OperationAnalysis.WithoutImplicitConversions(map.Arguments[0].Value).Type;
                ReportIfStrippable(invocation, ElementType(collection), ConstantName(invocation), true, report);
            }
        }

        /// <summary>
        /// Folds the constraint chain from its leftmost member, tracking the type each property is looked up on.
        /// <c>operand</c> is the type an And/Or/With after a resolved constraint returns to: the actual value, the element
        /// after a collection operator, or the type named by a type constraint.
        /// </summary>
        private void AnalyzeThat(IInvocationOperation invocation, System.Action<Diagnostic> report)
        {
            IArgumentOperation? expression = null;
            foreach (var argument in invocation.Arguments)
            {
                if (SymbolEqualityComparer.Default.Equals(argument.Parameter?.Type, _resolveConstraint))
                {
                    expression = argument;
                    break;
                }
            }

            if (expression is null)
            {
                return;
            }

            // That<TActual>(TActual, ...) and That<TActual>(ActualValueDelegate<TActual>, ...) both carry the actual
            // type as TActual; That(TestDelegate, ...) has none.
            var target = invocation.TargetMethod.TypeArguments.FirstOrDefault();
            var operand = target;
            foreach (var step in OperationAnalysis.ConstraintChain(expression.Value).Reverse())
            {
                var member = step switch
                {
                    IInvocationOperation call => call.TargetMethod.OriginalDefinition,
                    IPropertyReferenceOperation property => (ISymbol)property.Property.OriginalDefinition,
                    _ => null,
                };
                if (member is null)
                {
                    continue;
                }

                if (_propertySteps.TryGetValue(member, out var name))
                {
                    target = ReportIfStrippable(step, target, name ?? ConstantName(step), true, report)?.Type;
                }
                else if (_collectionOperators.Contains(member))
                {
                    target = operand = ElementType(target);
                }
                else if (_typeConstraints.Contains(member))
                {
                    target = operand = ((IInvocationOperation)step).TargetMethod.TypeArguments[0];
                }
                else if (_throwsExceptions.TryGetValue(member, out var exceptionType))
                {
                    target = operand = exceptionType;
                }
                else if (_conjunctions.Contains(member))
                {
                    target = operand;
                }
                else if (_by.Contains(member))
                {
                    ReportIfStrippable(step, ElementType(target), ConstantName(step), false, report);
                }
            }
        }

        /// <summary>
        /// Returns the property NUnit would find on <paramref name="target"/>, reporting it when it can be stripped,
        /// or null when the property cannot be resolved at compile time.
        /// </summary>
        private IPropertySymbol? ReportIfStrippable(IOperation step, ITypeSymbol? target, string? name,
            bool includeNonPublic, System.Action<Diagnostic> report)
        {
            var property = FindProperty(target, name, includeNonPublic);
            if (property is not null && !IsReferencedByNUnit(property))
            {
                report(Diagnostic.Create(Rule, OperationAnalysis.MemberNameLocation(step.Syntax),
                    $"{property.ContainingType.Name}.{property.Name}"));
            }

            return property;
        }

        private IPropertySymbol? FindProperty(ITypeSymbol? target, string? name, bool includeNonPublic)
        {
            if (target is null || name is null || target.SpecialType == SpecialType.System_Object ||
                target.TypeKind is TypeKind.Error or TypeKind.Dynamic or TypeKind.TypeParameter ||
                SymbolEqualityComparer.Default.Equals(target, _systemType))
            {
                return null;
            }

            var candidates = target.TypeKind == TypeKind.Interface
                ? new[] { target }.Concat(target.AllInterfaces)
                : BaseTypesAndSelf(target);
            return candidates
                .SelectMany(type => type.GetMembers(name).OfType<IPropertySymbol>())
                .FirstOrDefault(p => !p.IsStatic && !p.IsIndexer &&
                                     (includeNonPublic || p.DeclaredAccessibility == Accessibility.Public));
        }

        private static IEnumerable<ITypeSymbol> BaseTypesAndSelf(ITypeSymbol type)
        {
            for (ITypeSymbol? t = type; t is not null; t = t.BaseType)
            {
                yield return t;
            }
        }

        /// <summary>
        /// The linker keeps an override or an interface implementation of a getter it keeps, so those count as
        /// referenced too (MemoryStream.Length overrides Stream.Length, HashSet&lt;T&gt;.Count implements ICollection&lt;T&gt;.Count).
        /// </summary>
        private bool IsReferencedByNUnit(IPropertySymbol property)
        {
            for (var p = property; p is not null; p = p.OverriddenProperty)
            {
                if (_nunitReferenced.Contains(p.OriginalDefinition))
                {
                    return true;
                }
            }

            var type = property.ContainingType;
            return type.AllInterfaces
                .SelectMany(i => i.GetMembers(property.Name).OfType<IPropertySymbol>())
                .Any(member => _nunitReferenced.Contains(member.OriginalDefinition) &&
                               SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member),
                                   property));
        }

        private ITypeSymbol? ElementType(ITypeSymbol? type)
        {
            if (type is IArrayTypeSymbol array)
            {
                return array.ElementType;
            }

            return type is null
                ? null
                : new[] { type }.Concat(type.AllInterfaces)
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, _enumerableOfT))
                    ?.TypeArguments[0];
        }

        private static string? ConstantName(IOperation step) =>
            step is IInvocationOperation { Arguments.Length: 1 } invocation &&
            invocation.Arguments[0].Value.ConstantValue is { HasValue: true, Value: string name }
                ? name
                : null;

        private static ImmutableHashSet<ISymbol> Members(IEnumerable<INamedTypeSymbol?> types, params string[] names) =>
            types.OfType<INamedTypeSymbol>()
                .SelectMany(type => names.SelectMany(name => type.GetMembers(name)))
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
    }
}
