using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class MultipleInOneMethod
    {

        [Test]
        public void Test()
        {
            Assume.That(async () => await GetAsync(), Is.EqualTo(1)); // UTF2003
            Assert.That(() => GetAsync(), Is.EqualTo(1)); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
