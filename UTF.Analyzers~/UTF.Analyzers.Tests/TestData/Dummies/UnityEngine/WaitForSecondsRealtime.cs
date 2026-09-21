// Dummy of UnityEngine.WaitForSecondsRealtime. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine
{
    public class WaitForSecondsRealtime : CustomYieldInstruction
    {
        public WaitForSecondsRealtime(float time)
        {
        }

        public override bool keepWaiting => throw new NotImplementedException();
    }
}
