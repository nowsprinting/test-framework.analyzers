using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class AwaitThenAssertGood
    {
        [Test]
        public async Task Test()
        {
            var actual = await GetAsync();
            Assume.That(actual, Is.EqualTo(1));
            Assert.That(actual, Is.EqualTo(1));
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
