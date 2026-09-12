using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF5002
{
    public class OuterUnityTestAction : NUnitAttribute, IOuterUnityTestAction
    {
        public IEnumerator BeforeTest(ITest test)
        {
            yield return null;
        }

        public IEnumerator AfterTest(ITest test)
        {
            yield return null;
        }
    }

    public class OuterUnityTestActionTest
    {
        [Test]
        [OuterUnityTestAction]
        public void SyncTest()
        {
        }

        [Test]
        [OuterUnityTestAction]
        public async Task AsyncTest()
        {
            await Task.Yield();
        }

        [UnityTest]
        [OuterUnityTestAction]
        public IEnumerator CoroutineTest()
        {
            yield return null;
        }
    }
}
