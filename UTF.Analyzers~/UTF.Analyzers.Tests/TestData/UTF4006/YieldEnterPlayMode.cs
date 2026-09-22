using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldEnterPlayMode
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return new EnterPlayMode();
        }
    }
}
