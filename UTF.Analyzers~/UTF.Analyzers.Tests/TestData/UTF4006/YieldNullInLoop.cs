using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldNullInLoop
    {
        private bool _ready;

        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            while (!_ready)
            {
                yield return null;
            }
        }
    }
}
