// Dummy of UnityEngine.TestTools.EnterPlayMode. Declaration-only; see test-data-conventions.md.

using System;
using System.Collections;

namespace UnityEngine.TestTools
{
    public class EnterPlayMode : IEditModeTestYieldInstruction
    {
        public bool ExpectDomainReload => throw new NotImplementedException();

        public bool ExpectedPlaymodeState => throw new NotImplementedException();

        public IEnumerator Perform()
        {
            throw new NotImplementedException();
        }
    }
}
