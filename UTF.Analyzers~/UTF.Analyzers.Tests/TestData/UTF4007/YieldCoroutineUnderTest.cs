using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{

    public class YieldCoroutineUnderTest
    {
        [UnityTearDown]
        public IEnumerator TearDown()
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
