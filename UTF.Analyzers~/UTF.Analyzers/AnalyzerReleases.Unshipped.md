### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
UTF1001 | Structure | Error | TestCase and TestCaseSource attributes are not supported on coroutine-style test methods
UTF1002 | Structure | Error | Task<TResult> is not supported as a test method return type
UTF1003 | Structure | Error | Pairwise, Sequential, and Combinatorial attributes are not supported on coroutine-style test methods
UTF1004 | Structure | Error | OneTimeSetUp and OneTimeTearDown attributes are not supported on async methods
UTF2001 | Assertion | Error | Assert.ThrowsAsync, CatchAsync, and DoesNotThrowAsync are not supported
UTF2002 | Assertion | Error | Async delegates are not supported as the actual value of Throws constraints, Assert.Throws, Assert.Catch, and Assert.DoesNotThrow
UTF2003 | Assertion | Error | Async delegates are not supported as the actual value of the constraint model
