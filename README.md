# UTF.Analyzers

Roslyn analyzers for writing unit tests with Unity Test Framework.


## Diagnostics

| ID      | Category   | Description                                                                 |
|---------|------------|-----------------------------------------------------------------------------|
| UTF1xxx | Structure  | Test method signatures and attribute combinations                           |
| UTF2xxx | Assertion  | Misuse of assertions and constraints                                        |
| UTF3xxx | Suppressor | Suppressions of other analyzers' diagnostics that do not apply to UTF       |
| UTF4xxx | Style      | Code that works but is not recommended                                      |
| UTF5xxx | Extensions | Rules for authors of custom attributes, constraints, and comparers          |

See details: [Diagnostics Index](Documentation~/index.md)


## Installation

TBD


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