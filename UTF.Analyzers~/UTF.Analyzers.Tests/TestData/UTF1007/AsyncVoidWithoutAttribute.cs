using System.Threading.Tasks;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class AsyncVoidWithoutAttribute
    {
        public async void Reset()
        {
            await Task.Yield();
        }
    }
}
