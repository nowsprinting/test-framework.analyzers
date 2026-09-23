using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class UnityOneTimeTearDownOnVoid
    {
        [UnityOneTimeTearDown]
        public void OneTimeTearDown() // UTF1009
        {
        }
    }
}
