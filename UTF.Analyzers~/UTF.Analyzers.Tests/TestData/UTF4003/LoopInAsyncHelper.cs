using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInAsyncHelper
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await WaitForFlagAsync();
        }

        private async Task WaitForFlagAsync()
        {
            while (!_flag)   // UTF4003
            {
            }

            await Task.Yield();
        }
    }
}
