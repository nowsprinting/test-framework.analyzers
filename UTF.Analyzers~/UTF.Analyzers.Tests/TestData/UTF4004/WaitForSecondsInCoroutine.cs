using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class WaitForSecondsInCoroutine
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return new WaitForSeconds(1f);   // UTF4004
        }
    }
}
