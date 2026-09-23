// Dummy of Cysharp.Threading.Tasks.CancellationTokenSourceExtensions. Declaration-only; see test-data-conventions.md.

using System;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
    public static class CancellationTokenSourceExtensions
    {
        public static IDisposable CancelAfterSlim(this CancellationTokenSource cts, int millisecondsDelay,
            DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
        {
            throw new NotImplementedException();
        }
    }
}
