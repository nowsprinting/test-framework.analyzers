using System;
using System.Threading.Tasks;
using NUnit.Framework;
using static NUnit.Framework.Throws;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class UsingStaticThrows
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await FooAsync(), TypeOf<InvalidOperationException>()); // UTF2002
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
