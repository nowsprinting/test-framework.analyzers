using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{
    public class AsyncTestMethod
    {
        [Test]
        public async Task Test()
        {
            await Task.Yield();
        }
    }
}
