// Dummy of UnityEngine.MonoBehaviour. Declaration-only; see test-data-conventions.md.

using System;
using System.Collections;

namespace UnityEngine
{
    public class MonoBehaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            throw new NotImplementedException();
        }
    }
}
