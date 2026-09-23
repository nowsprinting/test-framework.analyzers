using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class OverrideOfUnitySetUpOnEnumerableBase
    {
        [UnitySetUp]
        public virtual IEnumerable SetUp() // UTF1009
        {
            yield return null;
        }
    }

    public class OverrideOfUnitySetUpOnEnumerable : OverrideOfUnitySetUpOnEnumerableBase
    {
        public override IEnumerable SetUp()
        {
            yield return null;
        }
    }
}
