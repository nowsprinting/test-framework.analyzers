using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class LoopInLambda
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            Func<Task> waitForFlag = async () =>
            {
                while (!_flag)   // UTF4001
                {
                    await UniTask.NextFrame();
                }
            };
            await waitForFlag();
        }
    }
}
