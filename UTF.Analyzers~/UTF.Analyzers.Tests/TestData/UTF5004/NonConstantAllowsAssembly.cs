using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class NonConstantAllowsAssembly : NUnitAttribute, ITestAction // UTF5004
    {
        private readonly ActionTargets _targets;

        public NonConstantAllowsAssembly(ActionTargets targets)
        {
            _targets = targets;
        }

        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets => _targets;
    }
}
