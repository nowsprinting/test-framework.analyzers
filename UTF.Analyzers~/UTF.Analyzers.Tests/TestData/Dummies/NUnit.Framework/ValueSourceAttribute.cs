// Dummy of NUnit.Framework.ValueSourceAttribute. Declaration-only; see test-data-conventions.md.
using System;

namespace NUnit.Framework;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
public class ValueSourceAttribute : Attribute
{
    public ValueSourceAttribute(string sourceName)
    {
    }
}
