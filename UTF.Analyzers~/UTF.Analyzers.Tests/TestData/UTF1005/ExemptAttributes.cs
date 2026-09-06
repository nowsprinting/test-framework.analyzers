using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class ExemptAttributes
    {
        [Test]
        [Repeat(3)]
        public async Task RepeatedAsyncTest()
        {
            await Task.Yield();
        }

        [UnityTest]
        [Retry(3)]
        public IEnumerator RetriedCoroutineTest()
        {
            yield return null;
        }

        [UnityTest]
        [MaxTime(1000)]
        public IEnumerator MaxTimeCoroutineTest()
        {
            yield return null;
        }

        [Test]
        [ParametrizedIgnore("b", 10)]
        public async Task ParametrizedIgnoreAsyncTest([Values("a", "b")] string someString, [Values(5, 10)] int someInt)
        {
            await Task.Yield();
        }
    }
}
