# UTF.Analyzers

[![Meta file check](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/metacheck.yml)
[![Build](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/build.yml/badge.svg)](https://github.com/nowsprinting/test-framework.analyzers/actions/workflows/build.yml)

Provides Roslyn analyzers for writing unit tests with Unity Test Framework.
Diagnoses API usage that could cause Unity Editor freezes or runtime errors before running tests.

## Required

* Unity 2022.3.12f1 or later


## Diagnostics

### Structure (UTF1xxx)

Rules about test method signatures and attribute combinations.

| Id | Title |
|----|-------|
| [UTF1001](Documentation~/rules/UTF1001.md) | TestCase and TestCaseSource attributes are not supported on coroutine-style test methods |
| [UTF1002](Documentation~/rules/UTF1002.md) | Task<TResult> is not supported as a test method return type |
| [UTF1003](Documentation~/rules/UTF1003.md) | Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods |
| [UTF1004](Documentation~/rules/UTF1004.md) | OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods |

### Assertion (UTF2xxx)

Rules about misuse of assertions and constraints.

| Id | Title |
|----|-------|

### Suppressor (UTF3xxx)

Suppressions of diagnostics reported by other analyzers that do not apply to Unity Test Framework.

| Id | Title |
|----|-------|

### Style (UTF4xxx)

Rules about code that works but is not recommended.

| Id | Title |
|----|-------|

### Extensions (UTF5xxx)

Rules for authors of custom attributes, constraints, and comparers.

| Id | Title |
|----|-------|


## Installation

This package is published to both UPM and NuGet. Choose one of the following.

### Install UPM package via [OpenUPM](https://openupm.com/)

1. Open the Project Settings window (**Editor > Project Settings**) and select **Package Manager** tab
2. Click **+** button under the **Scoped Registries** and enter the following settings:
    1. **Name:** `package.openupm.com`
    2. **URL:** `https://package.openupm.com`
    3. **Scope(s):** `com.nowsprinting` and `nunit.analyzers.unity`
3. Open the Package Manager window (**Window > Package Manager**) and select **My Registries** tab
4. Select **UTF.Analyzers** and click the **Install** button

> [!NOTE]\
> You do not need to add a reference to the test assembly definition file (asmdef).
> Because it's configured via an assembly definition reference file (asmref) to apply across all test assemblies.

> [!NOTE]\
> Installing this package will also install the following packages:
> - Unity Test Framework v1.4.6
> - NUnit.Analyzers v3.9.0
>
> If you do not wish to use these, please use the NuGet package instead.

### Install NuGet package via [NuGetForUnity]([NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity))

1. Open the NuGetForUnity window via **NuGet > Manage NuGet Packages**
2. Search "UTF.Analyzers" and click **Install**

### Install NuGet package via [UnityNuGet]([UnityNuGet](https://github.com/bdovaz/UnityNuGet)) (hosted on OpenUPM)

1. Install the package:

   ```bash
   openupm add org.nuget.utf.analyzers
   ```

2. Open the `.asmdef` of each assembly you want the analyzer to apply to, add `UTF.Analyzers_Unity` to its **Assembly Definition References**.

> [!TIP]\
> Analyzers installed via NuGetForUnity apply to all assemblies in the project (including those in the PackageCache), while analyzers installed via UnityNuGet apply only to the referenced assembly and any assemblies that depend on it.


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

### Run tests

```bash
cd Packages/com.nowsprinting.test-framework.analyzers/UTF.Analyzers~ && dotnet test
```