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
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
