using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class SyncDelegates
    {

        [Test]
        public void Test()
        {
            Assert.That(() => Get(), Is.EqualTo(1));
            Assert.That(Get, Is.EqualTo(1));
            Assume.That(() => Get(), Is.EqualTo(1));
        }

        private static int Get()
        {
            return 1;
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
