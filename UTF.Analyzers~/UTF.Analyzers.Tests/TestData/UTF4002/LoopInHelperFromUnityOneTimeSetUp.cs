using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class LoopInHelperFromUnityOneTimeSetUp
    {
        private bool _flag;

        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetUp()
        {
            yield return WaitForFlag();   // UTF4002
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
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
