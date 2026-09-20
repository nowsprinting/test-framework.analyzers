// Dummy of NUnit.Framework.TimeoutAttribute. Declaration-only; see test-data-conventions.md.
// IApplyToContext is omitted: the analyzer compares the attribute class only.

using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class TimeoutAttribute : NUnitAttribute
    {
        public TimeoutAttribute(int timeout)
        {
        }
    }
}
