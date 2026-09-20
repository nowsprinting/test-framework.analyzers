using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class AsyncVoidSetUp
    {
        private bool _flag;

        [SetUp]
        public async void SetUp()
        {
            await UniTask.WaitUntil(() => _flag);
        }

        [Test]
        public void Test()
        {
        }
    }
}
