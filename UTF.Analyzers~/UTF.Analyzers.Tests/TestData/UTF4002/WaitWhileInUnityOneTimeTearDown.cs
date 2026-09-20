using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class WaitWhileInUnityOneTimeTearDown
    {
        private bool _flag;

        [UnityOneTimeTearDown]
        public IEnumerator OneTimeTearDown()
        {
            yield return new WaitWhile(() => !_flag);   // UTF4002
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
