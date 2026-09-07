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

#### Suppressor rules

A suppressor (`DiagnosticSuppressor`) removes a diagnostic reported by another analyzer when it does not apply in the target environment.
It has no severity, message, or code fix, so adapt the template:

- Table: replace the `Severity` row with `Suppresses | <the other analyzer's rule ID>`. Keep `Category`, `Enabled`, and `CodeFix`.
- Below the table, write `Justification: "..."` instead of a message. This is the sentence Roslyn shows to the user for the suppression.
- Note under the table that the severity guidance above does not apply.
- Motivation: name the suppressed rule, the analyzer package and version you verified it against, and the primary-source fact that makes it inapplicable
  (e.g. the API the rule suggests does not exist in the library version the environment ships). If the rule has a code fix, state what applying it produces (typically a compile error).
- **Check the suppressed rule's default severity first.** Roslyn never hands a diagnostic whose `DefaultSeverity` is `Error` to a suppressor
  (`AnalyzerDriver.ApplyProgrammaticSuppressionsCore` filters on `d.DefaultSeverity != DiagnosticSeverity.Error`; the check is on the descriptor's
  default, so lowering the severity in `.editorconfig` / `.globalconfig` does not help). Look up the `defaultSeverity` in the other analyzer's source
  (or its docs) and, if it is `Error`, tell the user that a suppressor cannot work for this rule and stop; offer the alternatives instead
  (the user disables the rule with `dotnet_diagnostic.<ID>.severity = none`, or the rule is re-implemented as a Unity-aware analyzer of this package).
  Record the verified default severity in the Motivation section.
- Prefer a conditional suppression (suppress only while the inapplicability holds in the compilation) and state the condition under Notes as the exclusion condition.
- Replace "Bad" / "Good" with sections that make sense for a suppression, e.g. "Suppressed" (the code the other rule reports, with a comment saying it is suppressed)
  and "Not compilable in ..." (the code the other rule's fix would produce).

**How users disable a suppressor**: a suppressor cannot be disabled from `.editorconfig` or `.globalconfig`. Roslyn checks only
`CompilationOptions.SpecificDiagnosticOptions`, which is filled from ruleset files and the `-nowarn` compiler switch
([dotnet/roslyn#49727](https://github.com/dotnet/roslyn/issues/49727)). `#pragma warning disable` and `[SuppressMessage]` do not apply either.
In the Notes section, replace the `.editorconfig` example used by analyzer rules with both working methods:

```
-nowarn:<SUPPRESSION_ID>
```

in `csc.rsp` (Unity picks up `Assets/csc.rsp`), and

```xml
<Rule Id="<SUPPRESSION_ID>" Action="None" />
```

in a ruleset file (in Unity, `Default.ruleset` in `Assets/` or `<assembly name>.ruleset` next to the `.asmdef`).
Do not write that `dotnet_diagnostic.<SUPPRESSION_ID>.severity = none` works; it silently does nothing.

### 4. Update README.md

Add a row linking to the new file to the table of the corresponding category under "Diagnostics" in ../README.md.
