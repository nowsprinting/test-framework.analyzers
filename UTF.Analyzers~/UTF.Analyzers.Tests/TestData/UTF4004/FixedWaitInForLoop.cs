using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInForLoop
    {
        [UnityTest]
        public IEnumerator Test()
        {
            for (var i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.1f);   // UTF4004
            }
        }
    }
}
