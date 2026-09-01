# UTF.Analyzers

Roslyn analyzers for writing unit tests with Unity Test Framework.


## Diagnostics

TBD


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

TBD

### Release workflow

The release process is as follows:

1. Run **Actions > Create release pull request > Run workflow**
2. Merge created pull request

Then, will do the release process automatically by [Release](.github/workflows/release.yml) workflow.
After tagging, [OpenUPM](https://openupm.com/) retrieves the tag and updates it.

> [!CAUTION]\
> Do **NOT** manually operation the following operations:
> - Create a release tag
> - Publish draft releases

> [!CAUTION]\
> You must modify the package name to publish a forked package.

> [!TIP]\
> If you want to specify the version number to be released, change the version number of the draft release before running the "Create release pull request" workflow.
