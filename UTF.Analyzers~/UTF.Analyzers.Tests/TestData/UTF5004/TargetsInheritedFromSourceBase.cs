using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    public class TargetsInheritedFromSourceBase : TargetsInheritedFromSourceBaseAttribute // UTF5004
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class TargetsInheritedFromSourceBaseAttribute : NUnitAttribute, ITestAction // UTF5004
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets => ActionTargets.Suite;
    }
}
