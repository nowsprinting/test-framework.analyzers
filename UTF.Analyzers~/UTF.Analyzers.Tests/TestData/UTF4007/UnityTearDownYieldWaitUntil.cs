using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{

    public class UnityTearDownYieldWaitUntil
    {
        private bool _done;

        [UnityTearDown]
        public IEnumerator TearDown()   // UTF4007
        {
            yield return new WaitUntil(() => _done);
        }
    }
}
