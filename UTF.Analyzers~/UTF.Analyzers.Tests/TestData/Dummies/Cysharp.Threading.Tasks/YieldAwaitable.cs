// Dummy of Cysharp.Threading.Tasks.YieldAwaitable. Declaration-only; see test-data-conventions.md.

using System;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks
{
    public readonly struct YieldAwaitable
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
