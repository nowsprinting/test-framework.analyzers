using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class UnityTearDownAndUnitySetUpOnEnumerable
    {
        [UnityTearDown]
        [UnitySetUp]
        public IEnumerable SetUpAndTearDown() // UTF1009
        {
            yield return null;
        }
    }
}
