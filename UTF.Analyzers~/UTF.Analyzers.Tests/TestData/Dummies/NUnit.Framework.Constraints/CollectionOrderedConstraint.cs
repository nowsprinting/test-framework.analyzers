// Dummy of NUnit.Framework.Constraints.CollectionOrderedConstraint. Declaration-only; see test-data-conventions.md.

using System;

namespace NUnit.Framework.Constraints
{
    public class CollectionOrderedConstraint : CollectionConstraint
    {
        public CollectionOrderedConstraint Then => throw new NotImplementedException();

        public CollectionOrderedConstraint By(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
