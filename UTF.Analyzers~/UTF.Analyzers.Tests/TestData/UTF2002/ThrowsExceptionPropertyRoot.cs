using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsExceptionPropertyRoot
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await FooAsync(), Throws.Exception.TypeOf<InvalidOperationException>()); // UTF2002
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
