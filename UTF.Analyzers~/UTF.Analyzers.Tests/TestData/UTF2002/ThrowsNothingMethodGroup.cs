using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsNothingMethodGroup
    {
        [Test]
        public void Test()
        {
            Assert.That(BarAsync, Throws.Nothing); // UTF2002
        }

        private static async Task BarAsync()
        {
            await Task.Yield();
        }
    }
}
