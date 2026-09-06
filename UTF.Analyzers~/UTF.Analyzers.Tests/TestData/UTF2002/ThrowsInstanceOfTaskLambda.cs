using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsInstanceOfTaskLambda
    {
        [Test]
        public void Test()
        {
            Assert.That(() => FooAsync(), Throws.InstanceOf<Exception>().With.Message.Contains("boom")); // UTF2002
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
