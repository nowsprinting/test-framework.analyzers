using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class ThrowsConstraintInVariable
    {

        [Test]
        public void Test()
        {
            var constraint = Throws.TypeOf<InvalidOperationException>();
            Assert.That(async () => await GetAsync(), constraint); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
