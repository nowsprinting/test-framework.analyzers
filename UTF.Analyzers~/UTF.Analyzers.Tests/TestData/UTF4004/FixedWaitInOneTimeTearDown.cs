using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInOneTimeTearDown
    {
        [OneTimeTearDown]
        public void TearDown()
        {
            Thread.Sleep(100);   // UTF4004
        }
    }
}
