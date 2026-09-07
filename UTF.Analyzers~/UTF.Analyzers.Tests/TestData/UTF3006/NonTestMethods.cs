using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF3006
{
    public class NonTestMethods
    {
        [SetUp]
        public async Task SetUp() // VSTHRD200
        {
            await Task.Yield();
        }

        [TearDown]
        public async Task TearDown() // VSTHRD200
        {
            await Task.Yield();
        }

        public async Task CreateFixture() // VSTHRD200
        {
            await Task.Yield();
        }

        [Test]
        public async Task AddTwoPositiveNumbers() // VSTHRD200
        {
            async Task Prepare() // VSTHRD200
            {
                await Task.Yield();
            }

            await Prepare();
        }
    }
}
