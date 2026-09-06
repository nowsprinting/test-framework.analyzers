using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class ValueTaskAsyncLambda
    {

        [Test]
        public void Test()
        {
            Assert.That(async () => await GetValueTaskAsync(), Is.EqualTo(1)); // UTF2003
        }

        private static async ValueTask<int> GetValueTaskAsync()
        {
            await Task.Yield();
            return 1;
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
