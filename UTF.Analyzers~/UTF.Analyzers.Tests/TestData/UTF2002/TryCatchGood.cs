using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class TryCatchGood
    {
        [Test]
        public async Task ExceptionIsThrown()
        {
            try
            {
                await FooAsync();
            }
            catch (InvalidOperationException e)
            {
                Assert.That(e.Message, Does.Contain("boom"));
                return;
            }

            Assert.Fail("Expected InvalidOperationException was not thrown");
        }

        [Test]
        public async Task ExceptionIsNotThrown()
        {
            await BarAsync();
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }

        private static async Task BarAsync()
        {
            await Task.Yield();
        }
    }
}
