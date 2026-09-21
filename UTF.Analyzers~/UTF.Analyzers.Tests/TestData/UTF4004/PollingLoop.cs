using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class PollingLoop
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)
            {
                if (_flag)
                {
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
