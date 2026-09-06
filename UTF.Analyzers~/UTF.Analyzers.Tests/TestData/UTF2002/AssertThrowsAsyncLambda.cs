using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class AssertThrowsAsyncLambda
    {
        [Test]
        public void Test()
        {
            Assert.Throws<InvalidOperationException>(async () => await FooAsync()); // UTF2002
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
