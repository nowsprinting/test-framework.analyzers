using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInUnitySetUp
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return new WaitForSeconds(1f);   // UTF4004
        }
    }
}
