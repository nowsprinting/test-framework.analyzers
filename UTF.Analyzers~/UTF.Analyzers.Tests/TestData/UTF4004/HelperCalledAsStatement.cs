using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class HelperCalledAsStatement
    {
        [Test]
        public void Test()
        {
            Settle();
        }

        private void Settle()
        {
            Thread.Sleep(1000);
        }
    }
}
