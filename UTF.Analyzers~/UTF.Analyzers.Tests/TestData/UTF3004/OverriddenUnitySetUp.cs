using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3004
{
    public abstract class SetUpBase
    {
        [UnitySetUp]
        public abstract IEnumerator SetUp();
    }

    public class OverriddenUnitySetUp : SetUpBase
    {
        public override IEnumerator SetUp() // NUnit1028
        {
            yield return null;
        }

        [Test]
        public void MyTestMethod()
        {
        }
    }
}
