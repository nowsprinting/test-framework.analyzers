// Dummy of NUnit.Framework.TimeoutAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Interfaces;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class TimeoutAttribute : NUnitAttribute, IApplyToContext
    {
        public TimeoutAttribute(int timeout)
        {
        }

        public void ApplyToContext(ITestExecutionContext context)
        {
        }
    }
}
