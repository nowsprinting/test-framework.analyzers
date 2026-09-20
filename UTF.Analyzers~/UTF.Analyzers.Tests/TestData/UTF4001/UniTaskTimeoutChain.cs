using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class UniTaskTimeoutChain
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await UniTask.WaitUntil(() => _flag).Timeout(TimeSpan.FromSeconds(5));
            await UniTask.WaitUntil(() => _flag).TimeoutWithoutException(TimeSpan.FromSeconds(5));
        }
    }
}
