using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsTypeOfGenericTaskLambdaWithMessage
    {
        [Test]
        public void Test()
        {
            Assert.That(() => GetAsync(), Throws.TypeOf<InvalidOperationException>(), "message {0}", 1); // UTF2002
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
