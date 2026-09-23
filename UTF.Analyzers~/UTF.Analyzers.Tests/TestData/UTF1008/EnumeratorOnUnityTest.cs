using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class EnumeratorOnUnityTest
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
