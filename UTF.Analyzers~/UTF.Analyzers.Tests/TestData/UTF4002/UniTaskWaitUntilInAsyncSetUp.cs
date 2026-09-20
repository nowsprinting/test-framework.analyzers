using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class UniTaskWaitUntilInAsyncSetUp
    {
        private bool _flag;

        [SetUp]
        public async Task SetUp()
        {
            await UniTask.WaitUntil(() => _flag);   // UTF4002
        }

        [Test]
        public void Test()
        {
        }
    }
}
