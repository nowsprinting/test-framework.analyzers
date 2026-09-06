// Dummy of NUnit.Framework.TestAttribute. Declaration-only; see test-data-conventions.md.
using System;
using NUnit.Framework.Interfaces;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestAttribute : NUnitAttribute, ISimpleTestBuilder
    {
    }
}
