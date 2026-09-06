using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class DelegateVariable
    {

        [Test]
        public void Test()
        {
            ActualValueDelegate<Task<int>> del = GetAsync;
            Assert.That(del, Is.EqualTo(1)); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
