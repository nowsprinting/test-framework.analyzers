// Dummy of NUnit.Framework.ITestAction. Declaration-only; see test-data-conventions.md.

using NUnit.Framework.Interfaces;

namespace NUnit.Framework
{
    public interface ITestAction
    {
        void BeforeTest(ITest test);

        void AfterTest(ITest test);

        ActionTargets Targets { get; }
    }
}
