using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class TearDownOnSyncVoid
    {
        [TearDown]
        public void TearDown()
        {
        }
    }
}
