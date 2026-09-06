// Dummy of Cysharp.Threading.Tasks.UniTask. Declaration-only; see test-data-conventions.md.
// Only the awaitable pattern (GetAwaiter and its awaiter) is declared; the AsyncMethodBuilder attribute is omitted,
// so fixtures cannot write "async UniTask" lambdas and cover that shape with ValueTask from the reference assemblies instead.

using System;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks
{
    public readonly struct UniTask
    {
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
