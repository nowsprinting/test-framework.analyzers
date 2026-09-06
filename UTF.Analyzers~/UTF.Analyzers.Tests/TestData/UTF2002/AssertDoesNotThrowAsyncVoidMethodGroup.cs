using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class AssertDoesNotThrowAsyncVoidMethodGroup
    {
        [Test]
        public void Test()
        {
            Assert.DoesNotThrow(BazAsync); // UTF2002
        }

        private static async void BazAsync()
        {
            await Task.Yield();
        }
    }
}
