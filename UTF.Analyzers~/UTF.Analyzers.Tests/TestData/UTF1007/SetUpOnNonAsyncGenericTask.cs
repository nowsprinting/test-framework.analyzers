using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class SetUpOnNonAsyncGenericTask
    {
        [SetUp]
        public Task<int> SetUp() // UTF1007
        {
            return Task.FromResult(0);
        }
    }
}
