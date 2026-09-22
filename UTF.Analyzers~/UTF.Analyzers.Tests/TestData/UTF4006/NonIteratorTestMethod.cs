using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class NonIteratorTestMethod
    {
        [UnityTest]
        public IEnumerator Test()
        {
            return new NonIteratorTestMethodPlayer().FadeOut();
        }
    }

    public class NonIteratorTestMethodPlayer : MonoBehaviour
    {
        public IEnumerator FadeOut()
        {
            yield return null;
        }
    }
}
