using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldEnumeratorVariable
    {
        [UnityTest]
        public IEnumerator Test()
        {
            IEnumerator wait = WaitOneFrame();
            yield return wait;
        }

        private IEnumerator WaitOneFrame()
        {
            yield return null;
        }
    }
}
