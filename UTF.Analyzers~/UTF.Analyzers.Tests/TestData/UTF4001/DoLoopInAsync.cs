using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class DoLoopInAsync
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            do   // UTF4001
            {
                await UniTask.NextFrame();
            } while (!_flag);
        }
    }
}
