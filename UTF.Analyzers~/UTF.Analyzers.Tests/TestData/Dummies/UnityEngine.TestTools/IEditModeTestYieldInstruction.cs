// Dummy of UnityEngine.TestTools.IEditModeTestYieldInstruction. Declaration-only; see test-data-conventions.md.

using System.Collections;

namespace UnityEngine.TestTools
{
    public interface IEditModeTestYieldInstruction
    {
        bool ExpectDomainReload { get; }

        bool ExpectedPlaymodeState { get; }

        IEnumerator Perform();
    }
}
