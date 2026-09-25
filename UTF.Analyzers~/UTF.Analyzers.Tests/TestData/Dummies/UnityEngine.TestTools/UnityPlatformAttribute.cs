// Dummy of UnityEngine.TestTools.UnityPlatformAttribute. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework;

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UnityPlatformAttribute : NUnitAttribute
    {
        public RuntimePlatform[] include { get; set; } = Array.Empty<RuntimePlatform>();

        public RuntimePlatform[] exclude { get; set; } = Array.Empty<RuntimePlatform>();

        public UnityPlatformAttribute()
        {
        }

        public UnityPlatformAttribute(params RuntimePlatform[] include)
        {
            this.include = include;
        }
    }
}
