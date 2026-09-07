# UTF Analyzers

[![Meta file check](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/metacheck.yml)
[![Build](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/build.yml/badge.svg)](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/UTFAnalyzers)](https://www.nuget.org/packages/UTFAnalyzers)
[![openupm](https://img.shields.io/npm/v/com.nowsprinting.test-framework.analyzers?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.nowsprinting.test-framework.analyzers/)

This package provides Roslyn analyzers that help you write unit tests using the [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/index.html).
It diagnoses API usage that could cause Unity Editor freezes or runtime errors before running tests and suggests best practices.


## Required

* Unity 2022.3.12f1 or later


## Diagnostics

### Baseline version

The analyzers diagnose against the Unity Test Framework package v1.4.6 as the baseline; they do not diagnose bugs that exist only in earlier versions.
UTF v1.4.6 is the last version you can update independently as a UPM package.

Diagnostics remain even if the bug is fixed in UTF v1.5.0 or later.
If the bug is fixed in the version you are using, suppress the diagnostic in your project via `.globalconfig` or `.editorconfig`:

```ini
dotnet_diagnostic.UTFxxxx.severity = none
```

### Structure (UTF1xxx)

Rules about test method signatures and attribute combinations.

| Id | Title |
|----|-------|
| [UTF1001](Documentation~/rules/UTF1001.md) | TestCase and TestCaseSource attributes are not supported on coroutine-style test methods |
| [UTF1002](Documentation~/rules/UTF1002.md) | Task&lt;TResult&gt; is not supported as a test method return type |
| [UTF1003](Documentation~/rules/UTF1003.md) | Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods |
| [UTF1004](Documentation~/rules/UTF1004.md) | OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods |
| [UTF1005](Documentation~/rules/UTF1005.md) | Attributes implementing ICommandWrapper are not supported on async and coroutine-style test methods |

### Assertion (UTF2xxx)

Rules about misuse of assertions and constraints.

| Id | Title |
|----|-------|
| [UTF2001](Documentation~/rules/UTF2001.md) | Assert.ThrowsAsync, CatchAsync, and DoesNotThrowAsync are not supported |
| [UTF2002](Documentation~/rules/UTF2002.md) | Async delegates are not supported as the actual value of Throws constraints, Assert.Throws, Assert.Catch, and Assert.DoesNotThrow |
| [UTF2003](Documentation~/rules/UTF2003.md) | Async delegates are not supported as the actual value of the constraint model |
| [UTF2004](Documentation~/rules/UTF2004.md) | Only TestDelegate is supported as the actual value of the AllocatingGCMemory constraint |
| [UTF2005](Documentation~/rules/UTF2005.md) | DelayedConstraint is not supported |

### Suppressor (UTF3xxx)

Suppressions of diagnostics reported by other analyzers that do not apply to Unity Test Framework.

| Id | Title |
|----|-------|
| [UTF3001](Documentation~/rules/UTF3001.md) | Suppress NUnit2045 (Use Assert.Multiple) when Assert.Multiple is not available |
| [UTF3002](Documentation~/rules/UTF3002.md) | Suppress CS8618 (Non-nullable field or property is uninitialized) when the member is initialized in a UnitySetUp or UnityOneTimeSetUp method |
| [UTF3003](Documentation~/rules/UTF3003.md) | Suppress CA1001 (Types that own disposable fields should be disposable) on test fixtures with a UnityTearDown or UnityOneTimeTearDown method |
| [UTF3004](Documentation~/rules/UTF3004.md) | Suppress NUnit1028 (The non-test method is public) on UnitySetUp, UnityOneTimeSetUp, UnityTearDown, and UnityOneTimeTearDown methods |

### Style (UTF4xxx)

Rules about code that works but is not recommended.

| Id | Title |
|----|-------|

### Extensions (UTF5xxx)

Rules for authors of custom attributes, constraints, and comparers.

| Id | Title |
|----|-------|
| [UTF5001](Documentation~/rules/UTF5001.md) | ApplyToTest, ApplyToContext, and Wrap must not throw exceptions |


## Installation

This package is published to both UPM and NuGet. Choose one of the following.

### Install UPM package via [OpenUPM](https://openupm.com/)

1. Open the Project Settings window (**Editor > Project Settings**) and select **Package Manager** tab
2. Click **+** button under the **Scoped Registries** and enter the following settings:
    1. **Name:** `package.openupm.com`
    2. **URL:** `https://package.openupm.com`
    3. **Scope(s):** `com.nowsprinting` and `nunit.analyzers.unity`
3. Open the Package Manager window (**Window > Package Manager**) and select **My Registries** tab
4. Select **UTF Analyzers** and click the **Install** button

> [!TIP]\
> You do not need to add a reference to the test assembly definition file (asmdef).
> Because it's configured via an assembly definition reference file (asmref) to apply across all test assemblies.

> [!TIP]\
> Installing the UPM package will also install the following packages:
> - Unity Test Framework v1.4.6
> - NUnit.Analyzers v3.9.0
>
> These are not required. If you do not need them, install the NuGet package instead.

### Install NuGet package via [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)

1. Open the NuGetForUnity window via **NuGet > Manage NuGet Packages**
2. Search "UTFAnalyzers" and click **Install**


## License

MIT License


## How to contribute

Open an issue or create a pull request.

Be grateful if you could label the PR as `enhancement`, `bug`, `chore`, and `documentation`.
See [PR Labeler settings](.github/pr-labeler.yml) for automatically labeling from the branch name.


## How to development

### Clone repo as a embedded package

Clone this repository as a submodule under the Packages/ directory in your project.

```bash
git submodule add git@github.com:nowsprinting/test-framework.analyzers.git Packages/com.nowsprinting.test-framework.analyzers
```

### Run Tests

```bash
cd Packages/com.nowsprinting.test-framework.analyzers/UTF.Analyzers~
dotnet test
```

### Build

```bash
cd Packages/com.nowsprinting.test-framework.analyzers/UTF.Analyzers~
dotnet build -c Release UTF.Analyzers
```

After the Release build, `UTF.Analyzers.dll` is automatically copied to `UTF.Analyzers/`.
Please commit the copied DLL; it will be distributed as part of the UPM package.
