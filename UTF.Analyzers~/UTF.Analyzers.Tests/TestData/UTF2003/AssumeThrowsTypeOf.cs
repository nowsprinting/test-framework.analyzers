using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class AssumeThrowsTypeOf
    {

        [Test]
        public void Test()
        {
            Assume.That(async () => await GetAsync(), Throws.TypeOf<InvalidOperationException>()); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
