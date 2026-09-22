using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class TaskDelayWithConfigureAwait
    {
        [Test]
        public async Task Test()
        {
            await Task.Delay(100).ConfigureAwait(false);   // UTF4004
        }
    }
}
