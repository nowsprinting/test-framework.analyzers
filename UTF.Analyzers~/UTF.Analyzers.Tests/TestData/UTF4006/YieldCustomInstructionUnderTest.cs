using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4006
{

    public class YieldCustomInstructionUnderTest
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return new YieldCustomInstructionUnderTestWait();
        }
    }

    public class YieldCustomInstructionUnderTestWait : CustomYieldInstruction
    {
        public override bool keepWaiting => false;
    }
}
