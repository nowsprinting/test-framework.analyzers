using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class MethodTimeout
    {
        private bool _flag;

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator Test()
        {
            while (!_flag)
            {
                yield return null;
            }
        }
    }
}
