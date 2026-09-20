// Dummy of Cysharp.Threading.Tasks.UniTaskExtensions. Declaration-only; see test-data-conventions.md.

using System;

namespace Cysharp.Threading.Tasks
{
    public static class UniTaskExtensions
    {
        public static UniTask Timeout(this UniTask task, TimeSpan timeout, DelayType delayType = DelayType.DeltaTime,
            PlayerLoopTiming timeoutCheckTiming = PlayerLoopTiming.Update,
            System.Threading.CancellationTokenSource? taskCancellationTokenSource = null)
        {
            throw new NotImplementedException();
        }

        public static UniTask<bool> TimeoutWithoutException(this UniTask task, TimeSpan timeout,
            DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming timeoutCheckTiming = PlayerLoopTiming.Update,
            System.Threading.CancellationTokenSource? taskCancellationTokenSource = null)
        {
            throw new NotImplementedException();
        }
    }
}
