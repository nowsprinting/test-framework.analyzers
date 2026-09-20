using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class BoundedWaitUntilInUnitySetUp
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return new WaitUntil(() => _flag, TimeSpan.FromSeconds(5), () => { }, WaitTimeoutMode.Realtime);
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
