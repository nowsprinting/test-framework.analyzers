# UTF.Analyzers

Roslyn analyzers for writing unit tests with Unity Test Framework.


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

If you installed [openupm-cli](https://github.com/openupm/openupm-cli), run the command below

```bash
openupm add com.nowsprinting.test-framework.analyzers
```

Or open Package Manager window (Window | Package Manager) and add package from git URL

```
https://github.com/nowsprinting/com.nowsprinting.test-framework.analyzers.git
```

> [!NOTE]\
> You do not need to add a reference to the test assembly definition file (asmdef).
> Because it's configured via an assembly definition reference file (asmref) to apply across all test assemblies.

> [!NOTE]\
> Installing this package will also install the following packages:
> - Unity Test Framework v1.4.6
> - NUnit.Analyzers v3.9.0
>
> If you do not wish to use these, please use the NuGet package instead.


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