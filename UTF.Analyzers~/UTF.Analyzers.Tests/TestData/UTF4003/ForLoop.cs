using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class ForLoop
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            for (var i = 0; i < 10 && !_flag; i++)
            {
                Thread.Sleep(10);
            }

            foreach (var i in new[] { 1, 2 })
            {
            }
        }
    }
}
