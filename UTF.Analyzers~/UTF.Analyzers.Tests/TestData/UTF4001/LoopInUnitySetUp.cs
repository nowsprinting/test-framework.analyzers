using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class LoopInUnitySetUp
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            while (!_flag)
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
