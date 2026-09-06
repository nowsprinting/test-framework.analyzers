// Dummy of NUnit.Framework.RepeatAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RepeatAttribute : NUnitAttribute, IWrapTestMethod
    {
        public RepeatAttribute(int count)
        {
        }

        public TestCommand Wrap(TestCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
