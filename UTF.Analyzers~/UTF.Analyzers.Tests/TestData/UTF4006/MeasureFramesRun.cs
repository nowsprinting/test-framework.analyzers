using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class MeasureFramesRun
    {
        [UnityTest]
        [Unity.PerformanceTesting.Performance]
        public IEnumerator Test()
        {
            yield return Unity.PerformanceTesting.Measure.Frames().Run();
        }
    }
}
