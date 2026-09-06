// Dummy of UnityEngine.TestTools.UnityTestAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UnityTestAttribute : NUnitAttribute, ISimpleTestBuilder, ITestBuilder
    {
    }
}
