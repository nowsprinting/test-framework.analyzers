using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class AsyncTaskOnUnityTest
    {
        [UnityTest]
        public async Task Test() // UTF1008
        {
            await Task.Yield();
        }
    }
}
