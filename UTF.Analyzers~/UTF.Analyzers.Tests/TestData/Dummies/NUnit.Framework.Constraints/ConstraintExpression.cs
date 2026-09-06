// Dummy of NUnit.Framework.Constraints.ConstraintExpression. Declaration-only; see test-data-conventions.md.

using System;

namespace NUnit.Framework.Constraints
{
    public class ConstraintExpression
    {
        public ResolvableConstraintExpression Message => throw new NotImplementedException();

        public ConstraintExpression Not => throw new NotImplementedException();

        public ExactTypeConstraint TypeOf<TExpected>()
        {
            throw new NotImplementedException();
        }

        public ContainsConstraint Contains(string expected)
        {
            throw new NotImplementedException();
        }
    }
}
