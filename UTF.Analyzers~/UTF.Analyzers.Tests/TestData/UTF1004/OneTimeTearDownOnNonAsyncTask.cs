using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class OneTimeTearDownOnNonAsyncTask
    {
        [OneTimeTearDown] // UTF1004
        public Task OneTimeTearDown()
        {
            return Task.CompletedTask;
        }
    }
}
