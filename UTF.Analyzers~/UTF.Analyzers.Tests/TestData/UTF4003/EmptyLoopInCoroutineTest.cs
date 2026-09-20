using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class EmptyLoopInCoroutineTest
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)   // UTF4003
            {
            }

            yield return null;
        }
    }
}
