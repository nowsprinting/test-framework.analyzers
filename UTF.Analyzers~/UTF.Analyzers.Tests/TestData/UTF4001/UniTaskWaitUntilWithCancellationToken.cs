using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class UniTaskWaitUntilWithCancellationToken
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            using var cts = new CancellationTokenSource();
            await UniTask.WaitUntil(() => _flag, cancellationToken: cts.Token);   // UTF4001
        }
    }
}
