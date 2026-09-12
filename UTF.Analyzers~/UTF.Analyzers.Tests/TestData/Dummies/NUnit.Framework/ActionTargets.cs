// Dummy of NUnit.Framework.ActionTargets. Declaration-only; see test-data-conventions.md.

using System;

namespace NUnit.Framework
{
    [Flags]
    public enum ActionTargets
    {
        Default = 0,
        Test = 1,
        Suite = 2,
    }
}
