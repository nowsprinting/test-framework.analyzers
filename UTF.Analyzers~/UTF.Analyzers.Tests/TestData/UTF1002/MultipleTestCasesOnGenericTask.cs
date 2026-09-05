using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class MultipleTestCasesOnGenericTask
    {
        [TestCase(1, ExpectedResult = 2)]
        [TestCase(2, ExpectedResult = 3)]
        public async Task<int> Increment(int value) // UTF1002
        {
            await Task.Yield();
            return value + 1;
        }
    }
}
