using System.Collections.Generic;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class GenericEnumeratorOnUnityTest
    {
        [UnityTest]
        public IEnumerator<object> Test() // UTF1008
        {
            yield return null;
        }
    }
}
