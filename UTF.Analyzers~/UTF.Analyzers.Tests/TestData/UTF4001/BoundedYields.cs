using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class BoundedYields
    {
        private bool _flag;

        [UnityTest]
        public IEnumerator Test()
        {
            yield return null;
            yield return new WaitForSeconds(1f);
            yield return SceneManager.UnloadSceneAsync("Stage");
            yield return new WaitUntil(() => _flag, TimeSpan.FromSeconds(5), () => { }, WaitTimeoutMode.Realtime);
            yield return new WaitWhile(() => !_flag, TimeSpan.FromSeconds(5), () => { }, WaitTimeoutMode.Realtime);
            foreach (var frame in new[] { 1, 2, 3 })
            {
                yield return null;
            }
        }
    }
}
