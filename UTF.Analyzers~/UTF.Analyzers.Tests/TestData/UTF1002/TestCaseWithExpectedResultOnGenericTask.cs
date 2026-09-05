using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestCaseWithExpectedResultOnGenericTask
    {
        [TestCase(1, ExpectedResult = 2)]
        public async Task<int> Increment(int value) // UTF1002
        {
            await Task.Yield();
            return value + 1;
        }
    }
}
