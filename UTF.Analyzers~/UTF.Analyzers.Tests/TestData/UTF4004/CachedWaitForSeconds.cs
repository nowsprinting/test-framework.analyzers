using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class CachedWaitForSeconds
    {
        private readonly WaitForSeconds _oneSecond = new WaitForSeconds(1f);

        [UnityTest]
        public IEnumerator Test()
        {
            yield return _oneSecond;   // UTF4004
        }
    }
}
