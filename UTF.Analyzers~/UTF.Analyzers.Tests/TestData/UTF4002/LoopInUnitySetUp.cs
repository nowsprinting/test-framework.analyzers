using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class LoopInUnitySetUp
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            while (!_flag)   // UTF4002
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
