using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001;

public class TestCaseSourceOnCoroutine
{
    private static readonly object[] s_cases = { 1, 2 };

    [TestCaseSource(nameof(s_cases))]
    public IEnumerator MyCoroutineTest(int value) // UTF1001
    {
        yield return null;
    }
}
