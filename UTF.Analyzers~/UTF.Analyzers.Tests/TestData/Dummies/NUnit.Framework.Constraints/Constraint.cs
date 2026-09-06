// Dummy of NUnit.Framework.Constraints.Constraint. Declaration-only; see test-data-conventions.md.

using System;

namespace NUnit.Framework.Constraints
{
    public abstract class Constraint : IResolveConstraint
    {
        public ConstraintExpression With => throw new NotImplementedException();
    }
}
