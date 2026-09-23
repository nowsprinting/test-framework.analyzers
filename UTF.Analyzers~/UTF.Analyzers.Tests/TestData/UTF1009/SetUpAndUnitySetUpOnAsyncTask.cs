using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class SetUpAndUnitySetUpOnAsyncTask
    {
        [SetUp]
        [UnitySetUp]
        public async Task SetUp() // UTF1009
        {
            await Task.Yield();
        }
    }
}
