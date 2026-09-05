using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001
{
    public class TestCaseWithExpectedResultOnCoroutine
    {
        [TestCase(1, ExpectedResult = null)]
        public IEnumerator MyCoroutineTest(int value) // UTF1001
        {
            yield return null;
        }
    }
}
