// Dummy of NUnit.Framework.Throws. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
    public static class Throws
    {
        public static ResolvableConstraintExpression Exception => throw new NotImplementedException();

        public static ThrowsNothingConstraint Nothing => throw new NotImplementedException();

        public static ExactTypeConstraint ArgumentException => throw new NotImplementedException();

        public static ExactTypeConstraint TypeOf<TExpected>()
        {
            throw new NotImplementedException();
        }

        public static InstanceOfTypeConstraint InstanceOf<TExpected>()
        {
            throw new NotImplementedException();
        }
    }
}
