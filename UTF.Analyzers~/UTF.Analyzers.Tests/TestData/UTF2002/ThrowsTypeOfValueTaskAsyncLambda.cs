using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsTypeOfValueTaskAsyncLambda
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await FooAsync(), Throws.TypeOf<InvalidOperationException>()); // UTF2002
        }

        private static async ValueTask FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
