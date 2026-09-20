using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class SpinUntilInTest
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            SpinWait.SpinUntil(() => _flag);   // UTF4003
        }
    }
}
