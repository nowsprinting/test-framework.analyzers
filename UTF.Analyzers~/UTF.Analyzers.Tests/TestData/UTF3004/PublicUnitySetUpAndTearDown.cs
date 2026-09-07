using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3004
{
    public class PublicUnitySetUpAndTearDown
    {
        [UnitySetUp]
        public IEnumerator SetUp() // NUnit1028
        {
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown() // NUnit1028
        {
            yield return null;
        }

        [Test]
        public void MyTestMethod()
        {
        }
    }
}
