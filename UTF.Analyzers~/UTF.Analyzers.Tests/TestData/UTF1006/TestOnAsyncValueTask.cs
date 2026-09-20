using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestOnAsyncValueTask
    {
        [Test]
        public async ValueTask AwaitsYield() // UTF1006
        {
            await Task.Yield();
        }
    }
}
