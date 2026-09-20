using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class UniTaskTimeoutChain
    {
        private bool _flag;

        [SetUp]
        public async Task SetUp()
        {
            await UniTask.WaitUntil(() => _flag).Timeout(TimeSpan.FromSeconds(5));
            await UniTask.WaitUntil(() => _flag).TimeoutWithoutException(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void Test()
        {
        }
    }
}
