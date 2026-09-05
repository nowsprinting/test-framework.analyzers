using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1001
{
    public class UnityTestWithTestCaseOnCoroutine
    {
        [UnityTest]
        [TestCase(1)]
        public IEnumerator MyCoroutineTest(int value) // UTF1001
        {
            yield return null;
        }
    }
}
