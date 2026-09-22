using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldCoroutineUnderTest
    {
        [UnityTest]
        public IEnumerator Test()
        {
            var player = new YieldCoroutineUnderTestPlayer();
            yield return player.FadeOut();
        }
    }

    public class YieldCoroutineUnderTestPlayer : MonoBehaviour
    {
        public IEnumerator FadeOut()
        {
            yield return null;
        }
    }
}
