using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class RecursiveHelper
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            yield return Poll();   // UTF4001
        }

        private IEnumerator Poll()
        {
            while (!_flag)
            {
                yield return null;
            }

            yield return Poll();
        }
    }
}
