using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class HelperSharedByTests
    {
        private bool _ready;

        [UnityTest]
        public IEnumerator Test1()   // UTF4006
        {
            yield return WaitForReady();
        }

        [UnityTest]
        public IEnumerator Test2()   // UTF4006
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
