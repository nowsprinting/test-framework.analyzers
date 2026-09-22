using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldNull
    {
        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            yield return null;
        }
    }
}
