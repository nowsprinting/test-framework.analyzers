using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class MultipleWaits
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)   // UTF4001
            {
                yield return null;
            }

            yield return new WaitUntil(() => !_flag);   // UTF4001
        }
    }
}
