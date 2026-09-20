using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public abstract class OverrideOfUnitySetUpBase
    {
        [UnitySetUp]
        public abstract IEnumerator SetUp();
    }

    public class OverrideOfUnitySetUp : OverrideOfUnitySetUpBase
    {
        private bool _flag;

        public override IEnumerator SetUp()
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
