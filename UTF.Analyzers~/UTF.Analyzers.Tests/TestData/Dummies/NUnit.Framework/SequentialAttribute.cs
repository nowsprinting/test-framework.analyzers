// Dummy of NUnit.Framework.SequentialAttribute. Declaration-only; see test-data-conventions.md.
using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SequentialAttribute : Attribute
    {
    }
}
