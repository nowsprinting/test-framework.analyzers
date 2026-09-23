using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class EnumerableOnUnityTest
    {
        [UnityTest]
        public IEnumerable Test() // UTF1008
        {
            yield return null;
        }
    }
}
