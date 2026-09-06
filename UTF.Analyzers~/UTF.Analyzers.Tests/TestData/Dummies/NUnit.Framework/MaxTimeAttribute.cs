// Dummy of NUnit.Framework.MaxTimeAttribute. Declaration-only; see test-data-conventions.md.
using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MaxTimeAttribute : NUnitAttribute, IWrapTestMethod
    {
        public MaxTimeAttribute(int milliseconds)
        {
        }

        public TestCommand Wrap(TestCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
