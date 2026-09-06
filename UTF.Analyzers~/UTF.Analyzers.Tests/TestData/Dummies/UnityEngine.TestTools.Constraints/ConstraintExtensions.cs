// Dummy of UnityEngine.TestTools.Constraints.ConstraintExtensions. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Constraints;

namespace UnityEngine.TestTools.Constraints
{
    public static class ConstraintExtensions
    {
        public static AllocatingGCMemoryConstraint AllocatingGCMemory(this ConstraintExpression chain)
        {
            throw new NotImplementedException();
        }
    }
}
