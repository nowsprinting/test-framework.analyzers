// Dummy of UnityEngine.TestTools.Constraints.Is. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine.TestTools.Constraints
{
    public class Is : NUnit.Framework.Is
    {
        public static AllocatingGCMemoryConstraint AllocatingGCMemory()
        {
            throw new NotImplementedException();
        }
    }
}
