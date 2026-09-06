---
name: design-diagnostic
description: Creates a specification file for a new diagnostic rule (../Documentation~/rules/<DIAGNOSTIC_ID>.md). Use when asked to add, specify, or propose a diagnostic rule.
argument-hint: "[spec]"
license: MIT
metadata:
  author: Koji Hasegawa
---

# Creating a Diagnostic Rule Specification File

## Steps

### 1. Assign a diagnostic ID

`UTF` + four digits. The first digit is fixed per category. Refer to the diagnostics tables under "Diagnostics" in ../README.md for the existing categories and IDs, and confirm with the user before using a number range for a new category.

### 2. Verify against primary sources

If the diagnostic targets APIs of the Unity engine or other packages, follow [resources/unity-references.md](resources/unity-references.md) to consult primary sources (official documentation and source code) and confirm signatures and behavior before writing the spec.

### 3. Create the specification file

Create `../Documentation~/rules/<DIAGNOSTIC_ID>.md` based on `assets/TEMPLATE.md` in this skill.

- Write everything in English
- CodeFix is always False (project-wide policy: no code fixes are provided)
- Follow the terminology below
- Link to the Unity Manual without a version number: `https://docs.unity3d.com/Manual/...`, not `https://docs.unity3d.com/6000.4/Documentation/Manual/...`
- In Markdown (`../Documentation~/rules/*.md` and ../README.md), write `<` and `>` outside backticks as `&lt;` and `&gt;`, e.g. the title `Task&lt;TResult&gt; is not supported ...`. Bare angle brackets are rendered as HTML tags in the browser and disappear
- Do not mention the host Unity project, e.g. "NUnit.Analyzers 3.9.0 (the version shipped by the host project)". The package is distributed independently; state only the package and version you verified against

#### Terminology

| Use                                            | Instead of                                                            |
|------------------------------------------------|-----------------------------------------------------------------------|
| coroutine-style test method                    | coroutine test method, `IEnumerator` method                           |
| `async` modifier                               | `async` keyword                                                       |
| Use an 'async Task' test method instead.       | Use the `async` keyword instead.                                      |
| Apply `[Test]` to the method. (operation)      | place `[Test]` on, put `[Test]` on, the method carries `[Test]`       |
| `[Test]` marks the method as a test. (effect)  | `[Test]` is present on the method                                     |

- Title and message: no backticks. Quote identifiers with single quotes, like Roslyn: `Type '{0}' owns disposable field(s)`
- Title and message name the attributes they target (e.g. "TestCase and TestCaseSource attributes are not supported on ..."), not a category such as "method-level parameterized tests"
- When the title enumerates the types it targets (attributes, return types), the message outputs the type actually specified in the code as `'{0}'`, e.g. `'{0}' is not supported on coroutine-style test methods.` for `TestCaseAttribute`, or `'{0}' is not supported as a test method return type.` for `Task<int>`

#### Choosing the default severity

Anything that must be stopped before the test runs is an **Error**. Code that leads to a runtime error or a freeze is an Error,
e.g., "a test that waits for a state transition has no `Timeout` attribute". At Warning or below, a test that loops forever would be run without a timeout.

### 4. Update README.md

Add a row linking to the new file to the table of the corresponding category under "Diagnostics" in ../README.md.
