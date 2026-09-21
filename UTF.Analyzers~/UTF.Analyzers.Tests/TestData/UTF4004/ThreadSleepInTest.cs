using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class ThreadSleepInTest
    {
        [Test]
        public void Test()
        {
            Thread.Sleep(1000);   // UTF4004
        }
    }
}
