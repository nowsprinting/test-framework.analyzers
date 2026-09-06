using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class NonThrowsConstraintOnAsyncLambda
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await GetAsync(), Is.EqualTo(1));
            // Not reported by UTF2002 because the constraint does not start with Throws; UTF2003 covers this case.
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
