using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1003
{
    public class SequentialOnCoroutine
    {
        [Sequential] // UTF1003
        public IEnumerator MyCoroutineTest([Values(1, 2)] int a, [Values(3, 4)] int b)
        {
            yield return null;
        }
    }
}
