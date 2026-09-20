using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4002
{

    public class UnitySetUpReturningTask
    {
        private bool _flag;

        [UnitySetUp]
        public async Task SetUp()
        {
            await UniTask.WaitUntil(() => _flag);
        }

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
        }
    }
}
