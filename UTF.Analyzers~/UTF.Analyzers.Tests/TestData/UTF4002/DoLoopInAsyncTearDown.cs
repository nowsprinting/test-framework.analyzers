using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class DoLoopInAsyncTearDown
    {
        private bool _flag;

        [TearDown]
        public async Task TearDown()
        {
            do   // UTF4002
            {
                await UniTask.NextFrame();
            } while (!_flag);
        }

        [Test]
        public void Test()
        {
        }
    }
}
