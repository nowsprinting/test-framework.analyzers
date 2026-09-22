using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{
    public class UnityOneTimeSetUp
    {
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetUp()
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
