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

        // Returns the argument instead of throwing: UTF5001 follows the body of every ICommandWrapper.Wrap in the compilation,
        // and a throwing dummy would be reported in every fixture.
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
