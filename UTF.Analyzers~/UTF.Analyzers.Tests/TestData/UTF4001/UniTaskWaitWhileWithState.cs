using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class UniTaskWaitWhileWithState
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await UniTask.WaitWhile(this, t => !t._flag);   // UTF4001
        }
    }
}
