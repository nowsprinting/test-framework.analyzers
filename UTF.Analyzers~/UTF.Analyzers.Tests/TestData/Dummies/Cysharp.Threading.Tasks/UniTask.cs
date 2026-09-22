// Dummy of Cysharp.Threading.Tasks.UniTask. Declaration-only; see test-data-conventions.md.
// Only the awaitable pattern (GetAwaiter and its awaiter) is declared; the AsyncMethodBuilder attribute is omitted,
// so fixtures cannot write "async UniTask" lambdas and cover that shape with ValueTask from the reference assemblies instead.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
    public readonly struct UniTask
    {
        public static YieldAwaitable Yield()
        {
            throw new NotImplementedException();
        }

        public static UniTask NextFrame()
        {
            throw new NotImplementedException();
        }

        public static UniTask DelayFrame(int delayFrameCount, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            throw new NotImplementedException();
        }

        public static UniTask Delay(int millisecondsDelay, bool ignoreTimeScale = false,
            PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            throw new NotImplementedException();
        }

        public static UniTask WaitForSeconds(float duration, bool ignoreTimeScale = false,
            PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            throw new NotImplementedException();
        }

        public static UniTask WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            throw new NotImplementedException();
        }


        public static UniTask WaitWhile<T>(T state, Func<T, bool> predicate,
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            throw new NotImplementedException();
        }

        public static UniTask WaitUntilCanceled(CancellationToken cancellationToken,
            PlayerLoopTiming timing = PlayerLoopTiming.Update, bool completeImmediately = false)
        {
            throw new NotImplementedException();
        }

        public static UniTask<U> WaitUntilValueChanged<T, U>(T target, Func<T, U> monitorFunction,
            PlayerLoopTiming monitorTiming = PlayerLoopTiming.Update, IEqualityComparer<U>? equalityComparer = null,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
            where T : class
        {
            throw new NotImplementedException();
        }

        public Awaiter GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted => throw new NotImplementedException();

            public void GetResult()
            {
                throw new NotImplementedException();
            }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
