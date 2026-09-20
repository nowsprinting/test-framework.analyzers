using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class UnitySetUpOnAsyncTask
    {
        [UnitySetUp]
        public async Task SetUp()
        {
            await Task.Yield();
        }
    }
}
