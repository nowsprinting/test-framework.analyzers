using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldFixtureHelper
    {
        private bool _ready;

        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            yield return WaitForReady();
        }

        private IEnumerator WaitForReady()
        {
            while (!_ready)
            {
                yield return null;
            }
        }
    }
}
