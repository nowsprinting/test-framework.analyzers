using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestOnNonAsyncUniTask
    {
        [Test]
        public UniTask ReturnsUniTask() // UTF1006
        {
            return UniTask.NextFrame();
        }
    }
}
