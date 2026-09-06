// Dummy of NUnit.Framework.TestCaseAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Interfaces;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class TestCaseAttribute : NUnitAttribute, ITestBuilder
    {
        public TestCaseAttribute(params object?[]? arguments)
        {
        }

        public object? ExpectedResult { get; set; }
    }
}
