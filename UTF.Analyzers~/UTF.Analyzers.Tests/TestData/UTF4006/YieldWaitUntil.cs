using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldWaitUntil
    {
        private bool _ready;

        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            yield return new WaitUntil(() => _ready);
        }
    }
}
