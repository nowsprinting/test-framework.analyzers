using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class MultipleInOneMethod
    {
        [Test]
        public void Test()
        {
            Assert.That(BarAsync, Throws.Nothing); // UTF2002
            Assert.Throws<InvalidOperationException>(async () => await FooAsync()); // UTF2002
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
