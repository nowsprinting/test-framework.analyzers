using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestOnNonAsyncGenericTask
    {
        [Test]
        public Task<int> ReturnsValue() // UTF1002
        {
            return Task.FromResult(1);
        }
    }
}
