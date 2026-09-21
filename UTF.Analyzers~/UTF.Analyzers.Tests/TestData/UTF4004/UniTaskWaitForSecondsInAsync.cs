using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class UniTaskWaitForSecondsInAsync
    {
        [Test]
        public async Task Test()
        {
            await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);   // UTF4004
        }
    }
}
