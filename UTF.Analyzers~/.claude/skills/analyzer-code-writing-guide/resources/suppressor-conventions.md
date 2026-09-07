# Suppressor Conventions

How to implement and test a `DiagnosticSuppressor`, a Roslyn component that suppresses diagnostics reported by another analyzer
when they do not apply in a particular environment. Everything here that differs from a `DiagnosticAnalyzer` is a Roslyn
constraint, not a project preference; verified against the Roslyn source (`SuppressionDescriptor`, `SuppressionAnalysisContext`,
`AnalyzerManager`) and Microsoft.CodeAnalysis.Testing 1.1.2.

## What a suppressor is

- Subclass `DiagnosticSuppressor` (itself a `DiagnosticAnalyzer`, so the `[DiagnosticAnalyzer(LanguageNames.CSharp)]` attribute is still required).
- Declare one `SuppressionDescriptor` per suppressed rule: `Id` (the suppressor's own ID), `SuppressedDiagnosticId` (the other analyzer's rule ID), and `Justification` (a sentence shown to the user).
  There is no title, message, category, severity, or `helpLinkUri`.
- Expose them through `SupportedSuppressions`. Roslyn hands the suppressor only diagnostics whose ID is one of its `SuppressedDiagnosticId`s.
- **A diagnostic whose `DefaultSeverity` is `Error` can never be suppressed.** `AnalyzerDriver.ApplyProgrammaticSuppressionsCore` passes only
  diagnostics with `!IsSuppressed && !IsNotConfigurable() && DefaultSeverity != DiagnosticSeverity.Error` to `ReportSuppressions`. The check is on the
  descriptor's default, so an `.editorconfig` / `.globalconfig` severity override does not make an Error-by-default rule suppressible.
  Verify the suppressed rule's `defaultSeverity` in its source before writing a suppressor; if it is `Error`, do not write one.
- Override `ReportSuppressions(SuppressionAnalysisContext)`. There is no `Initialize`, no `RegisterCompilationStartAction`, and no per-node callback;
  the whole compilation's matching diagnostics arrive in `context.ReportedDiagnostics`.

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FooSuppressor : DiagnosticSuppressor
{
    public const string SuppressionId = "XXX3001";
    public const string SuppressedDiagnosticId = "OTHER1234";

    private static readonly SuppressionDescriptor Rule = new(
        SuppressionId,
        SuppressedDiagnosticId,
        justification: "Why the suppressed rule does not apply here.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // Resolve well-known symbols here, once per call; there is no compilation-start hook.
        var type = context.Compilation.GetTypeByMetadataName("Some.Namespace.Type");
        if (type is null || !type.GetMembers("Member").IsEmpty)
        {
            return;
        }

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            context.ReportSuppression(Suppression.Create(Rule, diagnostic));
        }
    }
}
```

- Prefer a **conditional** suppression (check the compilation for the fact that makes the rule inapplicable) over an unconditional one.
  An unconditional suppressor hides a valid diagnostic as soon as the environment changes, e.g. when the project references a newer library that does have the missing API.
- Per-diagnostic inspection is available when needed: `context.GetSemanticModel(diagnostic.Location.SourceTree)` and `diagnostic.Location.SourceSpan`.
  Check `context.CancellationToken` inside the loop either way.
- Symbol comparison rules are the same as for analyzers: `SymbolEqualityComparer.Default` on `OriginalDefinition`, `GetTypeByMetadataName` for well-known types, never `ContainingAssembly.Name`.

## Suppressor ID

Roslyn imposes no rule on `SuppressionDescriptor.Id` beyond uniqueness within the analyzer assembly. Two conventions exist in the wild:

- A separate prefix for suppressors (Microsoft.Unity.Analyzers: `USP0001`, `USP0002`, ...).
- A reserved range under the analyzer's normal prefix (NUnit.Analyzers: `NUnit3001`–`NUnit3004`, category "Suppressor").

Follow whichever the repository's README already defines for its Suppressor category.

## Release tracking

`AnalyzerReleases.Shipped.md` / `AnalyzerReleases.Unshipped.md` (RS2000, RS2007, RS2008) track `DiagnosticDescriptor`s only.
A `SuppressionDescriptor` gets no row; a clean build confirms no RS2xxx diagnostic is raised for it.

## How a suppressor is disabled by users

A suppressor **cannot** be disabled from `.editorconfig` or `.globalconfig`. `SuppressionDescriptor.IsDisabled` checks only
`CompilationOptions.SpecificDiagnosticOptions`, which is populated from ruleset files and the `-nowarn` compiler switch; analyzer config
files flow through `SyntaxTreeOptionsProvider` instead and are never consulted (open issue: [dotnet/roslyn#49727](https://github.com/dotnet/roslyn/issues/49727)).

- Working: `-nowarn:<SuppressionId>` in `csc.rsp` (or the compiler command line), or `<Rule Id="<SuppressionId>" Action="None" />` in a ruleset file.
- Not working: `dotnet_diagnostic.<SuppressionId>.severity = none` in `.editorconfig` / `.globalconfig`, `#pragma warning disable`, `[SuppressMessage]`.

Document the working methods in the rule's specification; do not copy the `.editorconfig` example used by analyzer rules.

## Testing a suppressor

### A second analyzer is required

A suppressor alone reports nothing, so the test must also run an analyzer that produces the suppressed diagnostic.
Subclass the harness's `CSharpAnalyzerTest`-based test type and override `GetDiagnosticAnalyzers()` to return both, then expect the
diagnostics with `.WithIsSuppressed(true)`:

```csharp
[Fact]
public async Task SuppressesOther1234()
{
    await Verifier.VerifyAsync(new Test(), "XXX3001/Case.cs",
        new DiagnosticResult(Other1234Stub.Rule).WithLocation(12, 13).WithIsSuppressed(true));
}

private sealed class Test : Verifier.Test
{
    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        yield return new Other1234Stub();
        yield return new FooSuppressor();
    }
}
```

Fixture convention: put the trailing `// <SuppressedDiagnosticId>` marker (e.g. `// OTHER1234`) on **every** line where the producer reports,
since every one of them is an expected (suppressed) diagnostic.

### Testing library version

Microsoft.CodeAnalysis.Testing **1.1.2 or later** is required. 1.1.1 drops suppressed diagnostics before verification, so a working suppressor
shows up as `Expected: N, Actual: 0`, indistinguishable from "never reported". 1.1.2 keeps programmatically suppressed diagnostics and
compares `IsSuppressed`. The library sets `reportSuppressedDiagnostics: true` itself; do not override `CreateCompilationOptions` for that.

### Choosing the producer: real analyzer or stub

Prefer the real analyzer from its NuGet package when it can see the test sources. Two gotchas when referencing an analyzer package as a plain assembly:

- The dll lives under `analyzers/dotnet/cs/` in the package, not `lib/`, so a `PackageReference` alone gives no compile reference. Use
  `<PackageReference Include="Pkg" Version="x" ExcludeAssets="all" GeneratePathProperty="true"/>` plus
  `<Reference Include="pkg.dll"><HintPath>$(PkgPkg)/analyzers/dotnet/cs/pkg.dll</HintPath></Reference>`.
- `ExcludeAssets` does not stop the SDK from running the package as an analyzer on the Tests project: the `ResolveLockFileAnalyzers` target
  still adds the dll as an `Analyzer` item, and its rules then fire on every dummy and fixture. Remove it afterwards:

  ```xml
  <Target Name="RemoveTestOnlyAnalyzers" AfterTargets="ResolveLockFileAnalyzers">
      <ItemGroup>
          <Analyzer Remove="@(Analyzer)" Condition="'%(Analyzer.NuGetPackageId)' == 'Pkg'"/>
      </ItemGroup>
  </Target>
  ```

Use a **test-only stub** instead when the real analyzer cannot recognize the dummies. **The stub must declare the same `DefaultSeverity` as the
real rule** (look it up in the analyzer's source and cite the version in the stub's doc comment): Roslyn drops Error-by-default diagnostics before
they reach any suppressor, so a stub declared at `Info` while the real rule is `Error` makes every suppressor test pass against a suppressor that
does nothing in a real build. Known case: NUnit.Analyzers identifies `Assert` by
`ContainingAssembly.Name == "nunit.framework"` (`ITypeSymbolExtensions.IsAssert`), and dummies are compiled into the test assembly,
so none of its rules ever fire against them. The stub lives in the Tests project, is `internal`, reports the same diagnostic ID at the
call sites the real rule would report, and needs no fidelity beyond that:

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class Other1234Stub : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new(
        // Same default severity as the real OTHER1234 (Other.Analyzers x.y.z); Error-by-default rules never reach a suppressor.
        FooSuppressor.SuppressedDiagnosticId, "Title", "Message", "Category", DiagnosticSeverity.Info, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(c =>
        {
            var invocation = (IInvocationOperation)c.Operation;
            if (invocation.TargetMethod.ContainingType.ToDisplayString() == "Some.Namespace.Type")
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
            }
        }, OperationKind.Invocation);
    }
}
```

Record in the rule's specification which producer the tests use and why.

### Testing the disabled case

Verify that `-nowarn` / ruleset disabling works by putting the suppressor ID into `SpecificDiagnosticOptions` and expecting the diagnostics
**without** `.WithIsSuppressed(true)`:

```csharp
protected override CompilationOptions CreateCompilationOptions()
{
    return base.CreateCompilationOptions().WithSpecificDiagnosticOptions(
        ImmutableDictionary<string, ReportDiagnostic>.Empty.Add(FooSuppressor.SuppressionId, ReportDiagnostic.Suppress));
}
```

Do not try to test `.editorconfig` / `.globalconfig` disabling; it does not work (see above).

### The "not suppressed because the condition does not hold" case

For a conditional suppressor, the negative case needs the compilation to contain the fact that makes the rule applicable again
(e.g. the missing API being present). With shared dummies this means either making the relevant dummy type `partial` and adding the member
from an extra source in that test only, or skipping the case. Decide with the user; either way, record the decision in the specification.
