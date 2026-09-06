// Dummy of NUnit.Framework.ValuesAttribute. Declaration-only; see test-data-conventions.md.
using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ValuesAttribute : Attribute
    {
        public ValuesAttribute(params object?[]? args)
        {
        }
    }
}
