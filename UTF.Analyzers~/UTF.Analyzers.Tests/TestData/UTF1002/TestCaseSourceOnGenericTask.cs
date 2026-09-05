using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestCaseSourceOnGenericTask
    {
        private static readonly object[] s_cases = { 1, 2 };

        [TestCaseSource(nameof(s_cases))]
        public async Task<int> Increment(int value) // UTF1002
        {
            await Task.Yield();
            return value + 1;
        }
    }
}
