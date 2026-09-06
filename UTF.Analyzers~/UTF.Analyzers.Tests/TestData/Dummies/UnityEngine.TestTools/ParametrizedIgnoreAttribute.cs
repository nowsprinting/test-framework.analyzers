// Dummy of UnityEngine.TestTools.ParametrizedIgnoreAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ParametrizedIgnoreAttribute : NUnitAttribute, IWrapTestMethod
    {
        public ParametrizedIgnoreAttribute(params object[] arguments)
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
