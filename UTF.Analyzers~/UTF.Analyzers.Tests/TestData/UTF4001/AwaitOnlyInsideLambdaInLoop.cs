using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class AwaitOnlyInsideLambdaInLoop
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            while (!_flag)
            {
                Func<Task>? bounded = async () => await UniTask.NextFrame();
                bounded = null;
            }

            await UniTask.NextFrame();
        }
    }
}
