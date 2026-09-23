using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class AsyncTaskOnTestAndUnityTest
    {
        [Test]
        [UnityTest]
        public async Task Test() // UTF1008
        {
            await Task.Yield();
        }
    }
}
