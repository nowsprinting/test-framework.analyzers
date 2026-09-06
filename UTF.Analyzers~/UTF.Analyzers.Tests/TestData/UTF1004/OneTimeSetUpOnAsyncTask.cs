using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class OneTimeSetUpOnAsyncTask
    {
        [OneTimeSetUp] // UTF1004
        public async Task OneTimeSetUp()
        {
            await Task.Yield();
        }
    }
}
