using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class LoopInNestedHelper
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await WaitForFlagAsync();   // UTF4001
        }

        private async Task WaitForFlagAsync()
        {
            await PollAsync();
        }

        private async Task PollAsync()
        {
            while (!_flag)
            {
                await UniTask.NextFrame();
            }
        }
    }
}
