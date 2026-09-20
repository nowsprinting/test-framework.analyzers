// Dummy of UnityEngine.WaitWhile. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine
{
    public sealed class WaitWhile : CustomYieldInstruction
    {
        public WaitWhile(Func<bool> predicate)
        {
        }

        public WaitWhile(Func<bool> predicate, TimeSpan timeout, Action onTimeout, WaitTimeoutMode timeoutMode)
        {
        }

        public override bool keepWaiting => throw new NotImplementedException();
    }
}
