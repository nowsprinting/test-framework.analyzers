using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class ForLoopInUnitySetUp
    {
        private bool _flag;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            for (var i = 0; i < 300 && !_flag; i++)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
