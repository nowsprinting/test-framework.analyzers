using System.Threading.Tasks;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class GenericTaskWithoutTestAttribute
    {
        public async Task<int> Increment(int value)
        {
            await Task.Yield();
            return value + 1;
        }
    }
}
