# UTF.Analyzers

Roslyn analyzers for writing unit tests with Unity Test Framework.


## Diagnostics

### Structure (UTF1xxx)

Rules about test method signatures and attribute combinations.

| Id | Title |
|----|-------|
| [UTF1001](UTF1001.md) | TestCase and TestCaseSource are not supported on coroutine test methods |
| [UTF1002](UTF1002.md) | `Task<TResult>` is not supported as a test method return type |
| [UTF1003](UTF1003.md) | Combining strategy attributes are not supported on coroutine test methods |

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
