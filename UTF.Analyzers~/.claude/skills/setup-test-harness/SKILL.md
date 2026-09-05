---
name: setup-test-harness
description: Builds the analyzer test harness (Tests project settings, TestDataVerifier, first dummies, smoke spike) once per repository. Use when UTF.Analyzers.Tests/TestDataVerifier.cs does not exist yet, typically from Step 1 of implement-diagnostic, or via /setup-test-harness.
license: MIT
metadata:
  author: Koji Hasegawa
---

# Setting Up the Test Harness

**One-time-per-repository setup that every later diagnostic implementation reuses.** Follow the items below in order only when
no analyzer has been implemented yet (i.e., only the two Rider template samples exist). If `UTF.Analyzers.Tests/TestDataVerifier.cs`
already exists, stop here; nothing to do.

Read `analyzer-code-writing-guide/resources/test-data-conventions.md` first for the design and the dummy conventions.
Test data is compiled twice (the Tests project build and the verifier's own compilation) and the verifier fails on even a single
unexpected diagnostic, so **spike this part before writing any analyzer logic.**

## Checklist

### 1. `UTF.Analyzers.Tests.csproj`

Make sure all of the following are present. Add whatever is missing.

```xml
<PropertyGroup>
    <!-- Fixtures must compile in Unity 6 (C# 9), and the verifier compiles with the Roslyn version brought in by the
         ProjectReference (4.3.0), whose default is C# 10. Pin the Tests project to the lower of the two; otherwise a fixture
         using newer syntax builds here but fails in Unity or in the verifier -->
    <LangVersion>9.0</LangVersion>
</PropertyGroup>

<ItemGroup>
    <!-- Dummies and fixtures are compiled here to type-check them against each other, and copied to the output
         directory so the verifier can read them as files at runtime -->
    <Compile Update="TestData/**/*.cs" CopyToOutputDirectory="PreserveNewest"/>
</ItemGroup>
```

Do not add any `<Reference Include="...">` items.

### 2. `TestDataVerifier.cs` (new)

Create `UTF.Analyzers.Tests/TestDataVerifier.cs`:

```csharp
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace UTF.Analyzers.Tests
{
    /// <summary>
    /// Reads a fixture from TestData/, runs the analyzer on it together with the dummies under TestData/Dummies/,
    /// and checks the reported diagnostics against the expected ones.
    /// Fixtures and dummies are real .cs files compiled by the Tests project, not string literals,
    /// so Unity/NUnit/UTF signatures are type-checked at build time.
    /// </summary>
    internal sealed class TestDataVerifier<TAnalyzer> : AnalyzerVerifier<TAnalyzer, TestDataVerifier<TAnalyzer>.Test, XUnitVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <param name="testDataPath">Path relative to TestData/, e.g. "UTF1001/ReturnsList.cs"</param>
        public static Task VerifyAsync(string testDataPath, params DiagnosticResult[] expected)
        {
            var test = new Test { TestCode = File.ReadAllText(Path.Combine(TestDataFiles.Root, testDataPath)) };
            // WithLocation(line, col) without a path resolves against DefaultFilePath ("/0/Test0.cs"), i.e. Sources[0].
            // The fixture must therefore be added first (via TestCode above) and the dummies only afterwards;
            // adding the dummies in the constructor would push the fixture to a later index and break every location assertion.
            foreach (var dummy in TestDataFiles.Dummies)
            {
                test.TestState.Sources.Add(dummy);
            }

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        internal sealed class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
        {
            public Test()
            {
                // Unity 6 compiles against .NET Standard 2.1. Net60 and later also work but would let fixtures
                // use APIs that do not exist in Unity.
                ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21;
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                // The Tests project has Nullable=enable while the verifier defaults to disabled.
                // Without this, a fixture containing string? builds in the Tests project but fails with CS8632 in the verifier.
                var options = (CSharpCompilationOptions)base.CreateCompilationOptions();
                return options.WithNullableContextOptions(NullableContextOptions.Enable);
            }
        }
    }

    /// <summary>
    /// Paths and dummy sources under TestData/. <see cref="TestDataVerifier{TAnalyzer}"/> gets a separate static field per type argument,
    /// so the directory scan lives in this non-generic type and runs once.
    /// </summary>
    internal static class TestDataFiles
    {
        public static string Root { get; } = Path.Combine(AppContext.BaseDirectory, "TestData");

        public static ImmutableArray<string> Dummies { get; } = Directory
            .EnumerateFiles(Path.Combine(Root, "Dummies"), "*.cs", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(File.ReadAllText)
            .ToImmutableArray();
    }
}
```

> [!NOTE]\
> The first `dotnet test` downloads the `NETStandard.Library.Ref` package for `ReferenceAssemblies.NetStandard.NetStandard21`
> from NuGet, so CI needs NuGet access (a normal `dotnet restore` already implies that).

### 3. Verify with a spike

1. Create the minimum dummies for `UnityEngine.MonoBehaviour` and `NUnit.Framework.TestAttribute`, following the "Dummies" section
   of `analyzer-code-writing-guide/resources/test-data-conventions.md`.
2. Write one smoke fixture that uses both, with a class name containing `MyCompany`
   (e.g., `public class MyCompanySmoke : MonoBehaviour { [Test] public void Foo() { } }`).
3. Run it through `TestDataVerifier<SampleSyntaxAnalyzer>` expecting exactly one `AB0001` at the class identifier's line/column.
   One expected diagnostic proves dummy resolution, the `Sources[0]` path assumption, and the `WithLocation` line numbers at once;
   a zero-diagnostic test would pass even if the fixture were never analyzed.
   (`SampleSyntaxAnalyzer` is the Rider template sample; if it has already been removed, use any existing analyzer and one of its Bad fixtures instead.)
4. Check:
   - the reference assemblies resolve
   - the dummies compile in both the Tests project and the verifier
   - no unexpected `CS####` is reported
   - `WithLocation` matches the actual file
5. **Do not proceed until this is green.**
6. Delete the spike fixture and test method; keep the dummies and the harness, then commit.
