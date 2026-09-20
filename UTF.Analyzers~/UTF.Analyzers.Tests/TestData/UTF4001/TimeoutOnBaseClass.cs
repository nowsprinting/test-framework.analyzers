using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    [Timeout(5000)]
    public abstract class TimeoutOnBaseClassBase
    {
    }

    public class TimeoutOnBaseClass : TimeoutOnBaseClassBase
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)   // UTF4001
            {
                yield return null;
            }
        }
    }
}
