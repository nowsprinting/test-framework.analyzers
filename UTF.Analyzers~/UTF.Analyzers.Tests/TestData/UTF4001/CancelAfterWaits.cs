using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class CancelAfterWaits
    {
        private bool _flag;

        [Test]
        public async Task Loop()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            while (!_flag)
            {
                await UniTask.Yield(cts.Token);
            }
        }

        [Test]
        public async Task LoopWithDelayInConstructor()
        {
            using var cts = new CancellationTokenSource(5000);
            while (!_flag)
            {
                await UniTask.Yield(cts.Token);
            }
        }

        [Test]
        public async Task WaitUntil()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            await UniTask.WaitUntil(() => _flag, cancellationToken: cts.Token);
        }
    }
}
