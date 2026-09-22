using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{
    public class AsyncSetUp
    {
        [SetUp]
        public async Task SetUp()
        {
            await Task.Yield();
        }

        [TearDown]
        public async Task TearDown()
        {
            await Task.Yield();
        }
    }
}
