using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class HelperOnOtherType
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return HelperOnOtherTypeUtil.WaitOneFrame();
        }
    }

    public static class HelperOnOtherTypeUtil
    {
        public static IEnumerator WaitOneFrame()
        {
            yield return null;
        }
    }
}
