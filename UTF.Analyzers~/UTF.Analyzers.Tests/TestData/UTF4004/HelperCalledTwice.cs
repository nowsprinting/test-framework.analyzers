using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class HelperCalledTwice
    {
        [UnityTest]
        public IEnumerator Test1()
        {
            yield return Settle();
        }

        [UnityTest]
        public IEnumerator Test2()
        {
            yield return Settle();
        }

        private IEnumerator Settle()
        {
            yield return new WaitForSeconds(1f);   // UTF4004
        }
    }
}
