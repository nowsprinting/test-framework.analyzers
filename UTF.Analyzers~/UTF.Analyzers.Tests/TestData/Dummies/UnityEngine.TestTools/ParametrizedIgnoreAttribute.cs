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

        public TestCommand Wrap(TestCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
