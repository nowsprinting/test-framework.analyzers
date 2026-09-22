using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class MultipleWaits
    {
        [Test]
        public async Task Test()
        {
            Thread.Sleep(100);   // UTF4004
            await Task.Delay(100);   // UTF4004
        }
    }
}
