// Dummy of NUnit.Framework.TestActionAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Interfaces;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class TestActionAttribute : Attribute, ITestAction
    {
        public virtual void BeforeTest(ITest test)
        {
        }

        public virtual void AfterTest(ITest test)
        {
        }

        public virtual ActionTargets Targets => ActionTargets.Default;
    }
}
