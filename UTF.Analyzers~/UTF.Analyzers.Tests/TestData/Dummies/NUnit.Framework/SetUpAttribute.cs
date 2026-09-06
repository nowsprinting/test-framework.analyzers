// Dummy of NUnit.Framework.SetUpAttribute. Declaration-only; see test-data-conventions.md.
using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SetUpAttribute : Attribute
    {
    }
}
