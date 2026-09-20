using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class HelperCalledTwice
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test1()
        {
            yield return WaitForFlag();
        }

        [UnityTest]
        public IEnumerator Test2()
        {
            yield return WaitForFlag();
        }

        private IEnumerator WaitForFlag()
        {
            while (!_flag)   // UTF4003
            {
            }

            yield break;
        }
    }
}
