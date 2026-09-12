using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly)]
    public class BothFlagsAllFour : NUnitAttribute, ITestAction
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;
    }
}
