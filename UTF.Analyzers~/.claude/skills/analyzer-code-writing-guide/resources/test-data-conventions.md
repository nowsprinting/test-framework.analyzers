# Test Data Conventions

How the test harness treats fixtures and dummies, and how to write dummies. Building the harness itself
(`UTF.Analyzers.Tests.csproj` settings, `TestDataVerifier.cs`, the smoke spike) is a one-time-per-repository job
covered by the `setup-test-harness` skill.

## Design

- **No local file references.** Tests run in CI, so the Tests project must not reference `UnityEngine*.dll`, `nunit.framework.dll`,
  or anything under `Library/`. No Unity installation is required.
- **Dummies instead of real assemblies.** The Unity, NUnit, and UTF types that fixtures use are declared as C# source files under
  `UTF.Analyzers.Tests/TestData/Dummies/`, with the real namespace, type name, and member signatures.
- **All test data is compilable C#.** Both dummies and fixtures are compiled by the Tests project (which type-checks fixtures against
  the dummies) and copied to the output directory so the verifier can read them at runtime.
- **The verifier feeds dummies as additional sources**, not as metadata references. The fixture is always the first source.

Test data is therefore compiled twice (the Tests project build and the verifier's own compilation). If the two disagree,
`CSharpAnalyzerTest` fails on even a single unexpected diagnostic.

## Dummies: `UTF.Analyzers.Tests/TestData/Dummies/`

- Layout: `TestData/Dummies/<Namespace>/<TypeName>.cs`, one type per file, e.g. `TestData/Dummies/NUnit.Framework/TestAttribute.cs`,
  `TestData/Dummies/UnityEngine.TestTools/UnityTestAttribute.cs`, `TestData/Dummies/UnityEngine/MonoBehaviour.cs`.
- Use the **real namespace and type name** so that `GetTypeByMetadataName` in the analyzer resolves the dummy exactly as it resolves the real type.
- Declare **only the members that fixtures actually use**, with signatures copied from primary sources
  (follow `design-diagnostic/resources/unity-references.md`). Attribute dummies must carry the real `[AttributeUsage]`; base types and interfaces that matter for detection must be preserved.
- **Declaration-only.** Method bodies are `throw new System.NotImplementedException();` or empty. Dummies are analyzed by the analyzer under test,
  so a dummy that trips a diagnostic fails every test that uses it.
- Never write dummies for `System.*`; those types come from the reference assemblies.
- When a later diagnostic needs a missing member, extend the existing dummy file. Never create a second dummy for the same type,
  and never declare one inside a fixture: every file under `TestData/` is compiled into the same Tests assembly, so a fixture-local
  `UnityEngine.Foo` collides (CS0101) with the dummy or with the next fixture that needs the same type, and in the verifier it would
  resolve as the real type without its signature ever having been checked against primary sources.

## Fixtures: `UTF.Analyzers.Tests/TestData/<DIAGNOSTIC_ID>/<CaseName>.cs`

- One case per file, file-scoped namespace `UTF.Analyzers.Tests.TestData.<DIAGNOSTIC_ID>;`
- Top-level type name = file name. When a case needs a pair of types (base/derived, interface/implementation, etc.), match only the primary type to the file name and give helper types a name unique within the file
- Self-contained. The verifier compiles that single file plus the dummies, so never reference types from sibling fixtures
- Reference Unity/NUnit/UTF APIs only through the dummies. If a dummy for the required type or member is missing, extend the existing dummy file (see "Dummies" above). Never declare a fixture-local type in a real namespace such as `UnityEngine` or `NUnit.Framework`
- No markup syntax (`{|ID:...|}`). It is invalid C# and incompatible with the .cs-file policy (test data is real, compilable code)
- In Bad fixtures, put a trailing comment containing only the diagnostic ID (e.g., `// UTF1001`) on the line where the reported location starts.
  No message, no reason, nothing else. Good fixtures and exclusion cases carry no comment
- No unused fields (`CS0169`/`CS0649` appear in both compilations and the verifier fails on any unexpected diagnostic). Unused private methods are safe (IDE inspections do not run in the verifier)
- Syntax up to the `<LangVersion>` pinned in the Tests project
