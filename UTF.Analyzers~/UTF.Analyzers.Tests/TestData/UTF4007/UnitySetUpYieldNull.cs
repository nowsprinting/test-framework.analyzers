using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{

    public class UnitySetUpYieldNull
    {
        [UnitySetUp]
        public IEnumerator SetUp()   // UTF4007
        {
            yield return null;
        }
    }
}
