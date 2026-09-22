using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class UniTaskDelayInAsync
    {
        [Test]
        public async Task Test()
        {
            await UniTask.Delay(1000);   // UTF4004
        }
    }
}
