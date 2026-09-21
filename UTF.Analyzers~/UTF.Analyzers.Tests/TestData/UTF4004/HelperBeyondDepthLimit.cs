using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class HelperBeyondDepthLimit
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return Depth1();
        }

        private IEnumerator Depth1()
        {
            yield return Depth2();
        }

        private IEnumerator Depth2()
        {
            yield return Depth3();
        }

        private IEnumerator Depth3()
        {
            yield return new WaitForSeconds(1f);
        }
    }
}
