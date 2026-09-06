using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class BothAttributesOnAsyncTask
    {
        [OneTimeSetUp] // UTF1004
        [OneTimeTearDown] // UTF1004
        public async Task SetUpAndTearDown()
        {
            await Task.Yield();
        }
    }
}
