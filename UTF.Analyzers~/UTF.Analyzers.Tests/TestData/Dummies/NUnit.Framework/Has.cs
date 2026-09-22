// Dummy of NUnit.Framework.Has. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
    public class Has
    {
        public static ConstraintExpression No => throw new NotImplementedException();

        public static ConstraintExpression All => throw new NotImplementedException();

        public static ConstraintExpression Some => throw new NotImplementedException();

        public static ResolvableConstraintExpression Length => throw new NotImplementedException();

        public static ResolvableConstraintExpression Count => throw new NotImplementedException();

        public static ConstraintExpression Exactly(int expectedCount)
        {
            throw new NotImplementedException();
        }

        public static ResolvableConstraintExpression Property(string name)
        {
            throw new NotImplementedException();
        }
    }
}
