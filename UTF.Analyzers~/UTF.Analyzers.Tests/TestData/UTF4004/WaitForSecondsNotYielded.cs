using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class WaitForSecondsNotYielded
    {
        private WaitForSeconds _wait;

        [UnityTest]
        public IEnumerator Test()
        {
            _wait = new WaitForSeconds(1f);
            yield return null;
        }

        public WaitForSeconds Wait => _wait;
    }
}
