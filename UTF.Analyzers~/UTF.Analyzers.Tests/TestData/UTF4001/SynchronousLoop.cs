using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class SynchronousLoop
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            while (!_flag)
            {
                _flag = true;
            }

            await UniTask.NextFrame();
        }
    }
}
