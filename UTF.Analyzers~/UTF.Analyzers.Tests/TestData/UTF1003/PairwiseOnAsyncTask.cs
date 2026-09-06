using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1003
{
    public class PairwiseOnAsyncTask
    {
        [Test]
        [Pairwise]
        public async Task MyAsyncTest([Values(1, 2)] int a, [Values(3, 4)] int b)
        {
            await Task.Yield();
        }
    }
}
