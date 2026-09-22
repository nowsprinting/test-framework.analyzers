// Dummy of NUnit.Framework.Constraints.ConstraintExpression. Declaration-only; see test-data-conventions.md.

using System;

namespace NUnit.Framework.Constraints
{
    public class ConstraintExpression
    {
        public ConstraintExpression Not => throw new NotImplementedException();

        public ConstraintExpression No => throw new NotImplementedException();

        public ConstraintExpression All => throw new NotImplementedException();

        public ConstraintExpression Some => throw new NotImplementedException();

        public ConstraintExpression None => throw new NotImplementedException();

        public ResolvableConstraintExpression Length => throw new NotImplementedException();

        public ResolvableConstraintExpression Count => throw new NotImplementedException();

        public ResolvableConstraintExpression Message => throw new NotImplementedException();

        public ResolvableConstraintExpression InnerException => throw new NotImplementedException();

        public ConstraintExpression With => throw new NotImplementedException();

        public CollectionOrderedConstraint Ordered => throw new NotImplementedException();

        public ConstraintExpression Exactly(int expectedCount)
        {
            throw new NotImplementedException();
        }

        public ResolvableConstraintExpression Property(string name)
        {
            throw new NotImplementedException();
        }

        public EqualConstraint EqualTo(object expected)
        {
            throw new NotImplementedException();
        }

        public GreaterThanConstraint GreaterThan(object expected)
        {
            throw new NotImplementedException();
        }

        public ExactTypeConstraint TypeOf<TExpected>()
        {
            throw new NotImplementedException();
        }

        public InstanceOfTypeConstraint InstanceOf<TExpected>()
        {
            throw new NotImplementedException();
        }

        public ContainsConstraint Contains(string expected)
        {
            throw new NotImplementedException();
        }
    }
}
