using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class SingleSleepOutsideLoop
    {
        [Test]
        public void Test()
        {
            Thread.Sleep(10);
        }
    }
}
