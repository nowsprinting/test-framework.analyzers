using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{
    public class TaskReturningUnitySetUp
    {
        [UnitySetUp]
        public async Task SetUp()   // not run by the framework
        {
            await Task.Yield();
        }
    }
}
