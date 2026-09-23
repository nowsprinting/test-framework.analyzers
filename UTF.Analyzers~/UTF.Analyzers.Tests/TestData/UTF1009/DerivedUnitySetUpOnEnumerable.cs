using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class DerivedUnitySetUpOnEnumerable
    {
        [MyUnitySetUp]
        public IEnumerable SetUp()
        {
            yield return null;
        }
    }

    public class MyUnitySetUpAttribute : UnitySetUpAttribute
    {
    }
}
