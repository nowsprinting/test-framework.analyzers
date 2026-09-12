using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    public class OverridesSourceBase : OverridesSourceBaseAttribute // UTF5004
    {
        public override ActionTargets Targets => ActionTargets.Test;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly)]
    public abstract class OverridesSourceBaseAttribute : NUnitAttribute, ITestAction
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
        }

        public virtual ActionTargets Targets => ActionTargets.Suite;
    }
}
