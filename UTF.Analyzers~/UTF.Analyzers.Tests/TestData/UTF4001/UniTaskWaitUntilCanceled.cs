using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class UniTaskWaitUntilCanceled
    {
        [Test]
        public async Task Test()
        {
            var source = new CancellationTokenSource();
            await UniTask.WaitUntilCanceled(source.Token);   // UTF4001
        }
    }
}
