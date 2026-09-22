using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class NonIteratorHelper
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return Wait();
        }

        private IEnumerator Wait()
        {
            return new NonIteratorHelperPlayer().FadeOut();
        }
    }

    public class NonIteratorHelperPlayer : MonoBehaviour
    {
        public IEnumerator FadeOut()
        {
            yield return null;
        }
    }
}
