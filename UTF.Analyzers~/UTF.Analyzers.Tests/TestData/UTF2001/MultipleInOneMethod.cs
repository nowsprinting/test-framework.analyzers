using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class MultipleInOneMethod
    {
        [Test]
        public void BothFreeze()
        {
            var e = Assert.ThrowsAsync<InvalidOperationException>(FooAsync); // UTF2001
            Assert.DoesNotThrowAsync(BarAsync); // UTF2001
            Console.WriteLine(e);
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
