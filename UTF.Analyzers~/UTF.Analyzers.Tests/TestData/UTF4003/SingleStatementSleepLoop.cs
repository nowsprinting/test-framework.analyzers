using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class SingleStatementSleepLoop
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            while (!_flag) Thread.Sleep(10);   // UTF4003
        }
    }
}
