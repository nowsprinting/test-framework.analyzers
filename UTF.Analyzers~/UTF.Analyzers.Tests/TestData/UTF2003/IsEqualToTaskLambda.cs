using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class IsEqualToTaskLambda
    {

        [Test]
        public void Test()
        {
            Assert.That(() => GetAsync(), Is.EqualTo(1)); // UTF2003
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
