using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class ThrowsAsyncGeneric
    {
        [Test]
        public void ThrowsAsyncFreezes()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await FooAsync()); // UTF2001
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
