using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class CancelAfterSlimWaits
    {
        private bool _flag;

        [Test]
        public async Task Loop()
        {
            using var cts = new CancellationTokenSource();
            using (cts.CancelAfterSlim(5000))
            {
                try
                {
                    while (!_flag)
                    {
                        await UniTask.Yield(cts.Token);
                    }
                }
                catch (OperationCanceledException e)
                {
                    if (e.CancellationToken == cts.Token)
                    {
                        Assert.Fail("Timeout waiting for the flag");
                    }
                }
            }
        }

        [Test]
        public async Task WaitUntil()
        {
            using var cts = new CancellationTokenSource();
            using (cts.CancelAfterSlim(5000))
            {
                await UniTask.WaitUntil(() => _flag, cancellationToken: cts.Token);
            }
        }
    }
}
