using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class EmptyLoopInUnitySetUp
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            while (!_flag)   // UTF4003
            {
            }

            yield return null;
        }

        [Test]
        public void Test()
        {
        }
    }
}
