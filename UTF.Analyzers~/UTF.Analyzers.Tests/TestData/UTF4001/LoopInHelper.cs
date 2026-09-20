using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class LoopInHelper
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
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
