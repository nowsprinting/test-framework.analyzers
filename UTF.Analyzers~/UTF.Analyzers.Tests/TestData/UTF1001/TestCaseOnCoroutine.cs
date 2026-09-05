using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001
{
    public class TestCaseOnCoroutine
    {
        [TestCase(1)] // UTF1001
        [TestCase(2)] // UTF1001
        public IEnumerator MyCoroutineTest(int value)
        {
            yield return null;
        }
    }
}
