using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class CancellationLoopWithoutDelay
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            using var cts = new CancellationTokenSource();
            while (!_flag)   // UTF4001
            {
                await UniTask.Yield(cts.Token);
            }
        }
    }
}
