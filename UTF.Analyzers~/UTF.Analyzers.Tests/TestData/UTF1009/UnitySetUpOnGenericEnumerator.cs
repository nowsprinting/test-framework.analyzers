using System.Collections.Generic;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class UnitySetUpOnGenericEnumerator
    {
        [UnitySetUp]
        public IEnumerator<object> SetUp() // UTF1009
        {
            yield return new object();
        }
    }
}
