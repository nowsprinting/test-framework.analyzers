using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestOnGenericValueTask
    {
        [Test]
        public async ValueTask<int> ReturnsValue()
        {
            await Task.Yield();
            return 1;
        }
    }
}
