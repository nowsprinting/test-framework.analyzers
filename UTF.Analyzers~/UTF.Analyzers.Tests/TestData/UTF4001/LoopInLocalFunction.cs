using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class LoopInLocalFunction
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await WaitForFlag();

            async Task WaitForFlag()
            {
                while (!_flag)   // UTF4001
                {
                    await UniTask.NextFrame();
                }
            }
        }
    }
}
