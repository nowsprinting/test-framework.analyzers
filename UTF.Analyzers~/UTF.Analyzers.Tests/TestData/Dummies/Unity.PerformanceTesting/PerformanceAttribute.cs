// Dummy of Unity.PerformanceTesting.PerformanceAttribute. Declaration-only; see test-data-conventions.md.

using System;

namespace Unity.PerformanceTesting
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class PerformanceAttribute : Attribute
    {
    }
}
