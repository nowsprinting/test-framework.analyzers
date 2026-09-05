using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestOnGenericTask
    {
        [Test]
        public async Task<int> ReturnsValue() // UTF1002
        {
            await Task.Yield();
            return 1;
        }
    }
}
