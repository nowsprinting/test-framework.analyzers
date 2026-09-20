using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class TearDownOnAsyncValueTask
    {
        [TearDown]
        public async ValueTask TearDown() // UTF1007
        {
            await Task.Yield();
        }
    }
}
