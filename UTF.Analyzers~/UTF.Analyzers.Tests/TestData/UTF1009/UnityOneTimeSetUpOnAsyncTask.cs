using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class UnityOneTimeSetUpOnAsyncTask
    {
        [UnityOneTimeSetUp]
        public async Task OneTimeSetUp() // UTF1009
        {
            await Task.Yield();
        }
    }
}
