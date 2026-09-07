using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3005
{
    public class NonTestMethods
    {
        [SetUp]
        public void Set_Up() // CA1707
        {
        }

        [UnitySetUp]
        public IEnumerator Unity_SetUp() // CA1707
        {
            yield return null;
        }

        public void Create_Fixture() // CA1707
        {
        }

        [Test]
        public void MyTestMethod()
        {
        }
    }
}
