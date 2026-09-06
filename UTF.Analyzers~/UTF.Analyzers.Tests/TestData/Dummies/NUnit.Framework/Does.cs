// Dummy of NUnit.Framework.Does. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
    public abstract class Does
    {
        public static SubstringConstraint Contain(string expected)
        {
            throw new NotImplementedException();
        }
    }
}
