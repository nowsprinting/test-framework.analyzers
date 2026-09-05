using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1003
{
    public class UnityTestWithValuesOnCoroutine
    {
        [UnityTest]
        public IEnumerator MyCoroutineTest([Values(1, 2)] int a, [Values(3, 4)] int b)
        {
            yield return null;
        }
    }
}
