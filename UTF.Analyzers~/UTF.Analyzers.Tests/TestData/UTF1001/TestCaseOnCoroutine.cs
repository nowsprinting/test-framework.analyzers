using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001;

public class TestCaseOnCoroutine
{
    [TestCase(1)]
    [TestCase(2)]
    public IEnumerator MyCoroutineTest(int value) // UTF1001
    {
        yield return null;
    }
}
