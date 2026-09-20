using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class OverrideOfUnitySetUp : OverrideOfUnitySetUpBase
    {
        private bool _flag;

        [Test]
        public void Test()
        {
        }

        public override IEnumerator SetUp()
        {
            while (!_flag)   // UTF4003
            {
            }

            yield return null;
        }
    }

    public abstract class OverrideOfUnitySetUpBase
    {
        [UnitySetUp]
        public abstract IEnumerator SetUp();
    }
}
