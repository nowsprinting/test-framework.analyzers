using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class UnityTearDownOnEnumerable
    {
        [UnityTearDown]
        public IEnumerable TearDown() // UTF1009
        {
            yield return null;
        }
    }
}
