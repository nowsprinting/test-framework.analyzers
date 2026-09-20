using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestCaseOnUniTask
    {
        [TestCase(1)]
        public UniTask ReturnsUniTask(int value) // UTF1006
        {
            return UniTask.NextFrame();
        }
    }
}
