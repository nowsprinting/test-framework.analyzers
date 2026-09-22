using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class NoYieldReturn
    {
        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            yield break;
        }
    }
}
