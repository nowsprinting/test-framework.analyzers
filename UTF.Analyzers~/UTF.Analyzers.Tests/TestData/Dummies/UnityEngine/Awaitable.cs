// Dummy of UnityEngine.Awaitable. Declaration-only; see test-data-conventions.md.
// Only the awaitable pattern (GetAwaiter and its awaiter) is declared; the AsyncMethodBuilder attribute is omitted,
// so fixtures return an Awaitable from a non-async method.

using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    public class Awaitable
    {
        public Awaiter GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public struct Awaiter : ICriticalNotifyCompletion
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
