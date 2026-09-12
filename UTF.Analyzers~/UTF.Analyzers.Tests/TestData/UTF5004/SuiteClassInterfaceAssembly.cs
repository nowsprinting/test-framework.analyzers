using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly)]
    public class SuiteClassInterfaceAssembly : NUnitAttribute, ITestAction
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
