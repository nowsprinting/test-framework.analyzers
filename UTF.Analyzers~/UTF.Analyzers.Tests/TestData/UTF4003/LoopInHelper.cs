using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInHelper
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
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
