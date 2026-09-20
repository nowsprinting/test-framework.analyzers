using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class MultipleWaits
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            while (!_flag)   // UTF4002
            {
                yield return null;
            }

            yield return new WaitUntil(() => _flag);   // UTF4002
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
