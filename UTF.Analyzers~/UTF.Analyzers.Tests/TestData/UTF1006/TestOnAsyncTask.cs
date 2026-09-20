using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestOnAsyncTask
    {
        [Test]
        public async Task AwaitsUniTask()
        {
            await UniTask.NextFrame();
        }
    }
}
