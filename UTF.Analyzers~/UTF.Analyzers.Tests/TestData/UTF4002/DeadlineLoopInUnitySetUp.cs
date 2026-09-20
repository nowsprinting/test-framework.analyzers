using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class DeadlineLoopInUnitySetUp
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var startTime = Time.time;
            while (!_flag && Time.time - startTime < 5.0f)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
