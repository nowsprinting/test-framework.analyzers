using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class WaitUntilInUnityTearDown
    {
        private bool _flag;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return new WaitUntil(() => _flag);   // UTF4002
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
