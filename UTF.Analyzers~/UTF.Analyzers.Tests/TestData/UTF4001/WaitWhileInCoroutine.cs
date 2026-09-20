using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class WaitWhileInCoroutine
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            yield return new WaitWhile(() => !_flag);   // UTF4001
        }
    }
}
