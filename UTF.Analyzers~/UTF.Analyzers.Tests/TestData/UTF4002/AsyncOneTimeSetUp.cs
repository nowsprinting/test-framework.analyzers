using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class AsyncOneTimeSetUp
    {
        private bool _flag;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await UniTask.WaitUntil(() => _flag);
        }

        [Test]
        public void Test()
        {
        }
    }
}
