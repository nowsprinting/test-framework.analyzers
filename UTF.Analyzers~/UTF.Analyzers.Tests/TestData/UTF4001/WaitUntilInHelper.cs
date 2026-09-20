using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class WaitUntilInHelper
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            yield return WaitForFlag();   // UTF4001
        }

        private IEnumerator WaitForFlag()
        {
            yield return new WaitUntil(() => _flag);
        }
    }
}
