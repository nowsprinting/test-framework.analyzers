using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{

    public class YieldFixtureHelper
    {
        [UnitySetUp]
        public IEnumerator SetUp()   // UTF4007
        {
            yield return WaitFrames();
        }

        private IEnumerator WaitFrames()
        {
            for (var i = 0; i < 3; i++)
            {
                yield return null;
            }
        }
    }
}
