using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class HelperSharedByTests
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test1()
        {
            yield return WaitForFlag();   // UTF4001
        }

        [UnityTest]
        public IEnumerator Test2()
        {
            yield return WaitForFlag();   // UTF4001
        }

        private IEnumerator WaitForFlag()
        {
            while (!_flag)
            {
                yield return null;
            }
        }
    }
}
