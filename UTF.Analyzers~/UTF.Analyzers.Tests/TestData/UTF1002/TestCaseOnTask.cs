using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestCaseOnTask
    {
        [TestCase(1, 2)]
        public async Task Increment(int value, int expected)
        {
            await Task.Yield();
        }
    }
}
