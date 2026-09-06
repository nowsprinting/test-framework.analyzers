using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class NewEqualConstraint
    {

        [Test]
        public void Test()
        {
            Assert.That(async () => await GetAsync(), new EqualConstraint()); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
