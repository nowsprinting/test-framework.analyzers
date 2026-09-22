// Dummy of NUnit.Framework.Is. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
    public abstract class Is
    {
        public static ConstraintExpression Not => throw new NotImplementedException();

        public static CollectionOrderedConstraint Ordered => throw new NotImplementedException();

        public static EqualConstraint EqualTo(object expected)
        {
            throw new NotImplementedException();
        }

        public static GreaterThanConstraint GreaterThan(object expected)
        {
            throw new NotImplementedException();
        }

        public static ExactTypeConstraint TypeOf<TExpected>()
        {
            throw new NotImplementedException();
        }
    }
}
