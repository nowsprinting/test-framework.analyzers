// Dummy of NUnit.Framework.Constraints.ResolvableConstraintExpression. Declaration-only; see test-data-conventions.md.

using System;

namespace NUnit.Framework.Constraints
{
    public class ResolvableConstraintExpression : ConstraintExpression, IResolveConstraint
    {
        public ConstraintExpression And => throw new NotImplementedException();

        public ConstraintExpression Or => throw new NotImplementedException();
    }
}
