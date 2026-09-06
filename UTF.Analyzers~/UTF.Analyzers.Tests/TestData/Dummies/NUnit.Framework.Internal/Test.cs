// Dummy of NUnit.Framework.Internal.Test. Declaration-only; see test-data-conventions.md.

using NUnit.Framework.Interfaces;

namespace NUnit.Framework.Internal
{
    public abstract class Test
    {
        public RunState RunState { get; set; }
    }
}
