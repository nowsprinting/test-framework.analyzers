using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class WaitUntilInCoroutine
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            yield return new WaitUntil(() => _flag);   // UTF4001
        }
    }
}
