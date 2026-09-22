using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{

    public class YieldEnterPlayMode
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return new EnterPlayMode();
        }
    }
}
