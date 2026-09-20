using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class UnityTestOnCoroutine
    {
        [UnityTest]
        public IEnumerator YieldsOnce()
        {
            yield return null;
        }
    }
}
