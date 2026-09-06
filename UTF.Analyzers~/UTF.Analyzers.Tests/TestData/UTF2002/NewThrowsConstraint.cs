using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class NewThrowsConstraint
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await FooAsync(), // UTF2002
                new ThrowsConstraint(new ExactTypeConstraint(typeof(InvalidOperationException))));
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
