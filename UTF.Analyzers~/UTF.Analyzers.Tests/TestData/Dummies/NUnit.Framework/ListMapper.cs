// Dummy of NUnit.Framework.ListMapper. Declaration-only; see test-data-conventions.md.

using System;
using System.Collections;

namespace NUnit.Framework
{
    public class ListMapper
    {
        public ListMapper(ICollection original)
        {
        }

        public ICollection Property(string name)
        {
            throw new NotImplementedException();
        }
    }
}
