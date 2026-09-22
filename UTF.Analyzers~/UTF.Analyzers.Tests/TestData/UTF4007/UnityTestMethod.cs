using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{
    public class UnityTestMethod
    {
        [UnityTest]
        public IEnumerator Test() // UTF4006, not UTF4007
        {
            yield return null;
        }
    }
}
