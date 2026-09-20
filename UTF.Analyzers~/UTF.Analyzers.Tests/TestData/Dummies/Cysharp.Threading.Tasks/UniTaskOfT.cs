// Dummy of Cysharp.Threading.Tasks.UniTask<T>. Declaration-only; see test-data-conventions.md.
// Only the awaitable pattern is declared, as in UniTask.cs.

using System;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks
{
    public readonly struct UniTask<T>
    {
        public Awaiter GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted => throw new NotImplementedException();

            public T GetResult()
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
