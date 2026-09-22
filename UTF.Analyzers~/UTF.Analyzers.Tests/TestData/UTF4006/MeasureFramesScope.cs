using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class MeasureFramesScope
    {
        [UnityTest]
        [Unity.PerformanceTesting.Performance]
        public IEnumerator Test()   // UTF4006
        {
            using (Unity.PerformanceTesting.Measure.Frames().Scope())
            {
                yield return null;
            }
        }
    }
}
