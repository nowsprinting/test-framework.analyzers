// Dummy of NUnit.Framework.TestCaseSourceAttribute. Declaration-only; see test-data-conventions.md.
using System;
using NUnit.Framework.Interfaces;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class TestCaseSourceAttribute : NUnitAttribute, ITestBuilder
    {
        public TestCaseSourceAttribute(string sourceName)
        {
        }
    }
}
