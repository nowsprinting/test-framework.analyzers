using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class CancelAfterLoopInAsyncSetUp
    {
        private bool _flag;

        [SetUp]
        public async Task SetUp()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            while (!_flag)
            {
                await UniTask.Yield(cts.Token);
            }
        }

        [Test]
        public void Test()
        {
        }
    }
}
