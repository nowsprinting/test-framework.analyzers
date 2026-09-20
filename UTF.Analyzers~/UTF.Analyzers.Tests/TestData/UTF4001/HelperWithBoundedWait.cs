using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class HelperWithBoundedWait
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return WaitFrames();
        }

        private IEnumerator WaitFrames()
        {
            for (var i = 0; i < 3; i++)
            {
                yield return null;
            }
        }
    }
}
