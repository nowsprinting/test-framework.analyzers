# UTF Analyzers

Roslyn analyzers and suppressors that diagnose test code written for Unity Test Framework. The vocabulary follows the [NUnit documentation](https://docs.nunit.org/) first, because the [Unity Test Framework manual](https://docs.unity3d.com/Manual/test-framework/test-framework-introduction.html) names the same concept differently from page to page. The manual's words are used for concepts NUnit does not have (Edit mode, Play mode, domain reload, the Unity attributes), and the package coins its own term where neither source has one (the three kinds of test method, async delegate, baseline, suppressor).

## Language

### Kinds of test method

**Test method**:
A method that NUnit runs as a test: one of its attributes implements `ITestBuilder` or `ISimpleTestBuilder`.
_Avoid_: test function, test case (a test case is one parameterized instance of a test method)

**Coroutine-style test method**:
A test method whose return type is exactly `IEnumerator`; Unity Test Framework runs it as a coroutine. Write "coroutine-style test method with `[UnityTest]`" when the attribute matters; the manual's "Unity test" means that case only.
_Avoid_: Unity test, coroutine test, IEnumerator test, UnityTest method

**Async Task test method**:
A test method whose return type is exactly `Task`.
_Avoid_: async test (NUnit's sense includes `Task<TResult>`, which Unity Test Framework does not run), asynchronous test, Task test

**Synchronous test method**:
A test method that returns `void` or a plain value. Neither the manual nor NUnit has a word for this.
_Avoid_: synchronous test, sync test, void test, regular test, NUnit test

**Parameterized test**:
A test method that produces several test cases from `[TestCase]`, `[TestCaseSource]`, `[ValueSource]`, `[Values]`, `[Range]`, or a combining strategy attribute.
_Avoid_: data-driven test

**Test case**:
One runnable instance of a test method, built by a test builder attribute.

### Where tests run

**Edit mode**:
The Editor state in which Edit mode tests run, driven by `EditorApplication.update`.
_Avoid_: Edit Mode, EditMode, editor mode

**Play mode**:
The Editor or Player state in which Play mode tests run, driven by the per-frame game loop.
_Avoid_: Play Mode, PlayMode, runtime mode

**Editor**:
The Unity Editor process. "The Editor freezes" means the main thread is blocked and no frame advances.
_Avoid_: Unity, IDE

**Player**:
A built executable of the project that runs Play mode tests outside the Editor.
_Avoid_: player build, build, device

**Main thread**:
The thread that runs the game loop, `EditorApplication.update`, coroutines, and every `await` continuation.
_Avoid_: UI thread, Unity thread

**Domain reload**:
The reload of all managed assemblies that happens on script recompilation and, by default, on entering Play mode.

**Synchronous run**:
A test run started with `-runSynchronously` or `ExecutionSettings.runSynchronously`. It excludes coroutine-style test methods with `[UnityTest]`, fixtures with a Unity setup and teardown method, and tests carrying an outer test action attribute.

### Setup and teardown

**Setup and teardown method**:
A method marked with `[SetUp]` or `[TearDown]` (per-test), `[OneTimeSetUp]` or `[OneTimeTearDown]` (one-time), or one of their Unity counterparts below. Singular forms are "setup method" and "teardown method".
_Avoid_: hook method, hook, lifecycle method, cleanup method (NUnit and the manual use "cleanup" only for build-time cleanup)

**Unity setup and teardown method**:
A setup and teardown method marked with `[UnitySetUp]`, `[UnityTearDown]`, `[UnityOneTimeSetUp]`, or `[UnityOneTimeTearDown]`. It returns `IEnumerator` and may yield instructions for the Editor.
_Avoid_: Unity hook, coroutine setup

**One-time setup and teardown method**:
A setup and teardown method that runs once per fixture: `[OneTimeSetUp]`, `[OneTimeTearDown]`, `[UnityOneTimeSetUp]`, `[UnityOneTimeTearDown]`. The others are per-test.
_Avoid_: fixture setup, class setup

**Async setup and teardown method**:
A setup and teardown method that returns `Task` or has the `async` modifier. Unity Test Framework supports it for `[SetUp]` and `[TearDown]` only; NUnit supports it for one-time methods too.

**Test fixture**:
A class that declares test methods or setup and teardown methods; "fixture" for short.
_Avoid_: test class, test suite

**Suite**:
A container node in the test tree: an assembly, a namespace, a fixture, or the method suite of a parameterized test method. An action attribute with `ActionTargets.Suite` runs once around it.
_Avoid_: test group, container

**Yield instruction for the Editor**:
A value a coroutine-style test method or Unity setup and teardown method yields to drive the Editor: `EnterPlayMode`, `ExitPlayMode`, `RecompileScripts`, `WaitForDomainReload`, or any `IEditModeTestYieldInstruction`.
_Avoid_: editor yield, custom yield instruction

### Actions outside the test

**Action attribute**:
An attribute implementing NUnit's `ITestAction`. Its `BeforeTest` and `AfterTest` are synchronous and run immediately around the test body or the suite, chosen by `Targets`. Never includes an outer test action attribute. Write "attribute implementing `ITestAction`" when precision matters, as the rule titles do.
_Avoid_: test action attribute, non-Unity action attribute

**Outer test action attribute**:
An attribute implementing Unity Test Framework's `IOuterUnityTestAction`, which has no NUnit counterpart. Its `BeforeTest` and `AfterTest` return `IEnumerator` and run before `[UnitySetUp]` and after `[UnityTearDown]`. Write "attribute implementing `IOuterUnityTestAction`" when precision matters.
_Avoid_: outer test action, outer action, OuterUnityTestAction attribute

**Command wrapper attribute**:
An attribute implementing `IWrapTestMethod` or `IWrapSetUpTearDown`. Its `Wrap` returns a `TestCommand` with a synchronous `Execute`. Write "attribute implementing `IWrapTestMethod` or `IWrapSetUpTearDown`" when precision matters.
_Avoid_: wrapper attribute, wrapper

**Combining strategy attribute**:
A `CombiningStrategyAttribute` that builds test cases from parameter values: `[Combinatorial]`, `[Pairwise]`, `[Sequential]`, and `[UnityTest]` itself.

**Execution order**:
The fixed sequence in which setup and teardown methods, action attributes, outer test action attributes, and command wrapper attributes run around a test, as listed on the manual's "Actions outside of tests" page.

### Assertions

**Constraint model**:
The `Assert.That(actual, constraint)` form of assertion, with constraints built from `Is`, `Has`, `Throws`, and Unity's own `Is`.
_Avoid_: fluent assertions, constraint-based assertions

**Classic model**:
The one-method-per-assertion form: `Assert.Throws`, `Assert.ThrowsAsync`, `Assert.Catch`, `Assert.DoesNotThrow`, and their kin.
_Avoid_: legacy asserts

**Actual value**:
The first argument of `Assert.That` or `Assume.That`: a value, an actual value delegate (`ActualValueDelegate<T>`, `() => value`), or a `TestDelegate`.
_Avoid_: subject, target

**Async delegate**:
An actual value delegate whose return type is awaitable, or that has the `async` modifier. Distinct from an async Task test method: this is an argument shape, not a test method.
_Avoid_: async lambda, Task delegate

### Analyzer outcomes

**Not runnable**:
NUnit's `RunState.NotRunnable`: the test is loaded but never executed and is reported as a failure.
_Avoid_: unrunnable, skipped (skipped is `RunState.Skipped` / `Ignored`)

**Baseline**:
Unity Test Framework 1.4.6, the version the analyzers diagnose against.

**Suppressor**:
A `DiagnosticSuppressor` that hides a diagnostic reported by another analyzer or the compiler when it does not apply under Unity Test Framework.
_Avoid_: suppression rule, filter
