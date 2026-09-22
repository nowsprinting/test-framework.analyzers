using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FrameWaits
    {
        private bool _flag;

        [Test]
        [Timeout(5000)]
        public async Task Async()
        {
            await UniTask.NextFrame();
            await UniTask.DelayFrame(5);
            await Task.Yield();
            await UniTask.WaitUntil(() => _flag);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator Coroutine()
        {
            yield return null;
            yield return new WaitUntil(() => _flag);
        }
    }
}
