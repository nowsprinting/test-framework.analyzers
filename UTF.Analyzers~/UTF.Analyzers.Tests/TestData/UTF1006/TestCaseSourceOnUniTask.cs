using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestCaseSourceOnUniTask
    {
        private static readonly object[] s_cases = { 1, 2 };

        [TestCaseSource(nameof(s_cases))]
        public UniTask ReturnsUniTask(int value) // UTF1006
        {
            return UniTask.NextFrame();
        }
    }
}
