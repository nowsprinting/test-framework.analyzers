using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class LoopInTestMethod
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
        }

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)
            {
                yield return null;
            }
        }
    }
}
