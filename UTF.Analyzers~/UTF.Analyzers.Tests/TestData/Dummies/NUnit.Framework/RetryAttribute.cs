// Dummy of NUnit.Framework.RetryAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RetryAttribute : NUnitAttribute, IWrapTestMethod
    {
        public RetryAttribute(int count)
        {
        }

        public TestCommand Wrap(TestCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
