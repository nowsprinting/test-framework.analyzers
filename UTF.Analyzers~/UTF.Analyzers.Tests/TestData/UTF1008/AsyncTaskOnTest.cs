using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class AsyncTaskOnTest
    {
        [Test]
        public async Task Test()
        {
            await Task.Yield();
        }
    }
}
