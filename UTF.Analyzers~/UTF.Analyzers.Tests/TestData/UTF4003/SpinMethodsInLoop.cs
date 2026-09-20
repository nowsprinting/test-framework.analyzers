using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class SpinMethodsInLoop
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            var spinner = new SpinWait();
            while (!_flag)   // UTF4003
            {
                Thread.Yield();
                Thread.SpinWait(10);
                spinner.SpinOnce();
                Thread.Sleep(System.TimeSpan.FromMilliseconds(1));
            }
        }
    }
}
