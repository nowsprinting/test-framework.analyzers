using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class QualifiedTargets : NUnitAttribute, ITestAction // UTF5004
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets => NUnit.Framework.ActionTargets.Suite;
    }
}
