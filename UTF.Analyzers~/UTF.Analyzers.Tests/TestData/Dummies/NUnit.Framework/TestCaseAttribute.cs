// Dummy of NUnit.Framework.TestCaseAttribute. Declaration-only; see test-data-conventions.md.
using System;

namespace NUnit.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class TestCaseAttribute : Attribute
{
    public TestCaseAttribute(params object?[]? arguments)
    {
    }

    public object? ExpectedResult { get; set; }
}
