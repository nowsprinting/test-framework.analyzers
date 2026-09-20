using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class YieldInLoop
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)
            {
                yield return null;
            }
        }
    }
}
