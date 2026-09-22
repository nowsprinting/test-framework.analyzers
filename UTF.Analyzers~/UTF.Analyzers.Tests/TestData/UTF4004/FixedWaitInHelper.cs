using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInHelper
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return Settle();
        }

        private IEnumerator Settle()
        {
            yield return new WaitForSeconds(1f);   // UTF4004
        }
    }
}
