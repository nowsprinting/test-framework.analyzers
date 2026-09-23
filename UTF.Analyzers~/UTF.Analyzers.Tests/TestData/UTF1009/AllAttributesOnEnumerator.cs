using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class AllAttributesOnEnumerator
    {
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetUp()
        {
            yield return null;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return null;
        }

        [UnityOneTimeTearDown]
        public IEnumerator OneTimeTearDown()
        {
            yield return null;
        }
    }
}
