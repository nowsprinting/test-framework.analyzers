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

`UTF` + four digits. The first digit is fixed per category. Refer to the diagnostics list in ../Documentation~/rules/index.md for the existing categories and IDs, and confirm with the user before using a number range for a new category.

### 2. Verify against primary sources

If the diagnostic targets APIs of the Unity engine or other packages, follow [resources/unity-references.md](resources/unity-references.md) to consult primary sources (official documentation and source code) and confirm signatures and behavior before writing the spec.

### 3. Create the specification file

Create `../Documentation~/rules/<DIAGNOSTIC_ID>.md` based on `assets/TEMPLATE.md` in this skill.

- Write everything in English
- CodeFix is always False (project-wide policy: no code fixes are provided)

> [!TIP]\
> Choosing the severity: anything that must be stopped before the test runs is an **Error**. Code that leads to a runtime
> error or a freeze is an Error, e.g., "a test that waits for a state transition has no `Timeout` attribute". At Warning or
> below, a test that loops forever would be run without a timeout.

### 4. Update index.md

Add a row to the table of the corresponding category in ../Documentation~/rules/index.md.
