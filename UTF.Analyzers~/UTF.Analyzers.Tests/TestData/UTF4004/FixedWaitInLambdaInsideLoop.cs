using System;
using System.Collections;
using System.Threading;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInLambdaInsideLoop
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)
            {
                Action sleep = () => Thread.Sleep(10);   // UTF4004
                sleep();
                yield return null;
            }
        }
    }
}
