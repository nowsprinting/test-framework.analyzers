using System.Threading.Tasks;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class AsyncTaskWithoutAttribute
    {
        public async Task OneTimeSetUp()
        {
            await Task.Yield();
        }
    }
}
