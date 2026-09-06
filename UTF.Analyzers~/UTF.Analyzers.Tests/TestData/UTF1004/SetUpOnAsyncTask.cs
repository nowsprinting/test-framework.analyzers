using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class SetUpOnAsyncTask
    {
        [SetUp]
        public async Task SetUp()
        {
            await Task.Yield();
        }
    }
}
