using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class MultipleWaits
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            while (!_flag)   // UTF4003
            {
            }

            SpinWait.SpinUntil(() => _flag);   // UTF4003
        }
    }
}
