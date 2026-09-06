using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class ThrowsConstraintOnAsyncLambda
    {

        [Test]
        public void Test()
        {
            Assert.That(async () => await GetAsync(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(async () => await GetAsync(), Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.That(async () => await GetAsync(), new ThrowsConstraint(new ExactTypeConstraint(typeof(InvalidOperationException))));
            Assert.That(async () => await GetAsync(), new ThrowsNothingConstraint());
            // Not reported by UTF2003 because the constraint starts with Throws or constructs a ThrowsConstraint; UTF2002 covers these cases.
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
