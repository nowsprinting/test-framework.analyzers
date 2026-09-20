using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInNestedHelper
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await Outer();
        }

        private async Task Outer()
        {
            await Inner();
        }

        private async Task Inner()
        {
            while (!_flag)   // UTF4003
            {
            }

            await Task.Yield();
        }
    }
}
