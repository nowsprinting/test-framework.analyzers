using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class TaskResultInsteadOfDelegate
    {
        [Test]
        public void Test()
        {
            Assert.That(FooAsync(), Throws.TypeOf<InvalidOperationException>());
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
