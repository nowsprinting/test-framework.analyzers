using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldBaseClassHelper : YieldBaseClassHelperBase
    {
        [UnityTest]
        public IEnumerator Test()   // UTF4006
        {
            yield return WaitOneFrame();
        }
    }

    public abstract class YieldBaseClassHelperBase
    {
        protected IEnumerator WaitOneFrame()
        {
            yield return null;
        }
    }
}
