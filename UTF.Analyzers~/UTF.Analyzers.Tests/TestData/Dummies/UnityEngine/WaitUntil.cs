// Dummy of UnityEngine.WaitUntil. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine
{
    public sealed class WaitUntil : CustomYieldInstruction
    {
        public WaitUntil(Func<bool> predicate)
        {
        }

        public WaitUntil(Func<bool> predicate, TimeSpan timeout, Action onTimeout, WaitTimeoutMode timeoutMode)
        {
        }

        public override bool keepWaiting => throw new NotImplementedException();
    }
}
