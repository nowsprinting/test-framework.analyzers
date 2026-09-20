using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class BoundedAwaits
    {
        [Test]
        public async Task Test()
        {
            await UniTask.Delay(1000);
            await UniTask.NextFrame();
            await UniTask.DelayFrame(5);
            await Task.Yield();
            for (var i = 0; i < 10; i++)
            {
                await UniTask.Yield();
            }
        }
    }
}
