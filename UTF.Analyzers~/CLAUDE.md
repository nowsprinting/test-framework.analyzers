# Development Guidelines

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Roslyn analyzers for writing unit tests with Unity Test Framework, distributed as a UPM package.
The analyzer is built with the .NET SDK (not Unity) and the resulting dll is shipped inside the UPM package.

## Directory Structure

This directory (`UTF.Analyzers~/`) is the .NET solution root. The trailing `~` hides it from Unity's asset importer.
The parent directory is the UPM package root.

- `UTF.Analyzers/` # Analyzer project (netstandard2.0)
- `UTF.Analyzers.Tests/` # xUnit tests using Microsoft.CodeAnalysis.Testing (net10.0)
- `UTF.Analyzers.Sample/` # Sample project that references the analyzer for manual verification (net10.0)
- `../UTF.Analyzers/` # UPM-facing directory. Contains the built `UTF.Analyzers.dll` and an `.asmref` to `UnityEngine.TestRunner`
- `../Documentation~/` # Package documentation published to https://nowsprinting.github.io/test-framework.analyzers/

## Test Project Policy

Tests must run on CI without a local Unity installation, so the test project does not reference any local dll.
Unity, NUnit, and UTF APIs that analyzers depend on are provided as dummy sources under `UTF.Analyzers.Tests/TestData/Dummies/`,
declared with the real namespace, type name, and member signatures.

Test data (the code an analyzer inspects) is placed as individual `.cs` files inside the test project
(`UTF.Analyzers.Tests/TestData/<DIAGNOSTIC_ID>/`) rather than as string literals, so that the compiler guarantees every fixture compiles.

## Diagnostics

Diagnostic IDs use the `UTF` prefix with the category encoded in the first digit:

| Range   | Category   | Description                                                                 |
|---------|------------|-----------------------------------------------------------------------------|
| UTF1xxx | Structure  | Test method signatures and attribute combinations                           |
| UTF2xxx | Assertion  | Misuse of assertions and constraints                                        |
| UTF3xxx | Suppressor | Suppressions of other analyzers' diagnostics that do not apply to UTF       |
| UTF4xxx | Style      | Code that works but is not recommended                                      |
| UTF5xxx | Extensions | Rules for authors of custom attributes, constraints, and comparers          |

When adding or changing a diagnostic:

1. Add the rule to `UTF.Analyzers/AnalyzerReleases.Unshipped.md` (required by RS2007/RS2008 release tracking)
2. Write a documentation page for the rule under `../Documentation~/rules/`
3. Add a row linking to that page in the matching category table under "Diagnostics" in `../README.md`
4. Add xUnit tests in `UTF.Analyzers.Tests/`

## Build

Build the analyzer in Release configuration:

```bash
dotnet build -c Release UTF.Analyzers
```

The `CopyToUnityPackage` target in `UTF.Analyzers.csproj` copies `UTF.Analyzers.dll` into `../UTF.Analyzers/` after a Release build.
Commit the copied dll; it is what the UPM package ships.
Do not commit `bin/` or `obj/`.

Unity requires the `RoslynAnalyzer` asset label on `UTF.Analyzers.dll.meta` to load the dll as an analyzer. Keep the label when the meta file is regenerated.

## Run Tests

```bash
dotnet test
```

## Language

All files, commit messages, GitHub Issues, and Pull Requests must be written in English.

## Branch Naming

Prefix branch names with the PR category:

- `feature/`: New features
- `fix/`: Bug fixes
- `chore/`: Tests, workflows, documentation, etc.

## Release

Do not create tags or publish releases manually.
Run **Actions > Create release pull request > Run workflow** and merge the created pull request; the Release workflow handles the rest.
