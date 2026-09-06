using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsConstraintInVariable
    {
        [Test]
        public void Test()
        {
            var constraint = Throws.TypeOf<InvalidOperationException>();
            Assert.That(async () => await FooAsync(), constraint); // UTF2002
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
