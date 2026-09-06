using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class ThrowsConstraintFromParameter
    {

        [Test]
        public void Test()
        {
            AssertWith(Throws.TypeOf<InvalidOperationException>());
        }

        private static void AssertWith(IResolveConstraint constraint)
        {
            Assert.That(async () => await GetAsync(), constraint); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
