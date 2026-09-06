using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class OneTimeTearDownOnGenericTask
    {
        [OneTimeTearDown] // UTF1004
        public Task<int> OneTimeTearDown()
        {
            return Task.FromResult(1);
        }
    }
}
