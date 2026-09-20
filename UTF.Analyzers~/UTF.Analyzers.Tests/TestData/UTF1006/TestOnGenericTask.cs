using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestOnGenericTask
    {
        [Test]
        public async Task<int> ReturnsValue()
        {
            await Task.Yield();
            return 1;
        }
    }
}
