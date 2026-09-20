using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class UniTaskWaitUntilInAsync
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await UniTask.WaitUntil(() => _flag);   // UTF4001
        }
    }
}
