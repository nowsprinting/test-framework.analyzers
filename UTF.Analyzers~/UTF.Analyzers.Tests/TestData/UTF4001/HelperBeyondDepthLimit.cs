using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class HelperBeyondDepthLimit
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await Depth1Async();
        }

        private async Task Depth1Async()
        {
            await Depth2Async();
        }

        private async Task Depth2Async()
        {
            await Depth3Async();
        }

        private async Task Depth3Async()
        {
            while (!_flag)
            {
                await UniTask.NextFrame();
            }
        }
    }
}
