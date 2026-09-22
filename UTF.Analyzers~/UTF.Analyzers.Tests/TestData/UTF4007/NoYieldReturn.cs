using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{

    public class NoYieldReturn
    {
        [UnityTearDown]
        public IEnumerator TearDown()   // UTF4007
        {
            yield break;
        }
    }
}
