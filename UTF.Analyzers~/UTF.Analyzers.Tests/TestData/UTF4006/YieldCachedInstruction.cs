using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldCachedInstruction
    {
        private readonly WaitForSeconds _oneSecond = new WaitForSeconds(1f);

        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            yield return _oneSecond;
        }
    }
}
