using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class UniTaskWaitUntilWithCancellationToken
    {
        private bool _flag;

        [SetUp]
        public async Task SetUp()
        {
            using var cts = new CancellationTokenSource();
            await UniTask.WaitUntil(() => _flag, cancellationToken: cts.Token);   // UTF4002
        }

        [Test]
        public void Test()
        {
        }
    }
}
