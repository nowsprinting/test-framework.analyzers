using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldStartCoroutine
    {
        [UnityTest]
        public IEnumerator Test()
        {
            var player = new YieldStartCoroutinePlayer();
            yield return player.StartCoroutine(player.FadeOut());
        }
    }

    public class YieldStartCoroutinePlayer : MonoBehaviour
    {
        public IEnumerator FadeOut()
        {
            yield return null;
        }
    }
}
