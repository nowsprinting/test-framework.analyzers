using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class HelperWithTimeoutChain
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await WaitForFlagAsync();
        }

        private async Task WaitForFlagAsync()
        {
            await UniTask.WaitUntil(() => _flag).Timeout(TimeSpan.FromSeconds(5));
        }
    }
}
