using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3004
{
    public class PublicUnityOneTimeSetUpAndTearDown
    {
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetUp() // NUnit1028
        {
            yield return null;
        }

        [UnityOneTimeTearDown]
        public IEnumerator OneTimeTearDown() // NUnit1028
        {
            yield return null;
        }

        [Test]
        public void MyTestMethod()
        {
        }
    }
}
