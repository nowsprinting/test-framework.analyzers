using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class UnityOneTimeSetUpOnCoroutine
    {
        [UnityOneTimeSetUp]
        public IEnumerator UnityOneTimeSetUp()
        {
            yield return null;
        }
    }
}
