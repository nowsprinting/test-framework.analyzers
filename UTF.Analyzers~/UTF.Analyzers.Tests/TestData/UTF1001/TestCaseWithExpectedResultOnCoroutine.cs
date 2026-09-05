using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001
{
    public class TestCaseWithExpectedResultOnCoroutine
    {
        [TestCase(1, ExpectedResult = null)] // UTF1001
        public IEnumerator MyCoroutineTest(int value)
        {
            yield return null;
        }
    }
}
