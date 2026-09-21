using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInLocalFunction
    {
        [Test]
        public void Test()
        {
            Settle();

            void Settle()
            {
                Thread.Sleep(100);   // UTF4004
            }
        }
    }
}
