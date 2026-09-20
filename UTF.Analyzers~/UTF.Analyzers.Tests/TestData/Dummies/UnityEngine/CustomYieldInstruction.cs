// Dummy of UnityEngine.CustomYieldInstruction. Declaration-only; see test-data-conventions.md.

using System;
using System.Collections;

namespace UnityEngine
{
    public abstract class CustomYieldInstruction : IEnumerator
    {
        public abstract bool keepWaiting { get; }

        public object Current => throw new NotImplementedException();

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
