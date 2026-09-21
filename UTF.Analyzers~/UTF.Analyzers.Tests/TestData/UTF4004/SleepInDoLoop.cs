using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class SleepInDoLoop
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            do
            {
                Thread.Sleep(10);
            } while (!_flag);
        }
    }
}
