using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1003
{
    public class SequentialOnVoid
    {
        [Test]
        [Sequential]
        public void MySyncTest([Values(1, 2)] int a, [Values(3, 4)] int b)
        {
        }
    }
}
