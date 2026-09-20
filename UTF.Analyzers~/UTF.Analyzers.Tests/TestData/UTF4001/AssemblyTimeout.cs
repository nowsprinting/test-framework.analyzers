using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

[assembly: Timeout(5000)]

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class AssemblyTimeout
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            while (!_flag)
            {
                yield return null;
            }
        }
    }
}
