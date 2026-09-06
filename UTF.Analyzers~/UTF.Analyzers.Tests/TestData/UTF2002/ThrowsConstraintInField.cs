using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsConstraintInField
    {
        private static readonly IResolveConstraint Constraint = Throws.InstanceOf<Exception>();

        [Test]
        public void Test()
        {
            Assert.That(async () => await FooAsync(), Constraint); // UTF2002
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
