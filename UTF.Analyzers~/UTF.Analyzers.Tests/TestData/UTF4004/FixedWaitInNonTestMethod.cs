using System.Threading;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInNonTestMethod
    {
        public void NotATest()
        {
            Thread.Sleep(1000);
        }
    }
}
