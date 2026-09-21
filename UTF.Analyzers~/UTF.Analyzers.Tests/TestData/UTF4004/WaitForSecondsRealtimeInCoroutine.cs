using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class WaitForSecondsRealtimeInCoroutine
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return new WaitForSecondsRealtime(1f);   // UTF4004
        }
    }
}
