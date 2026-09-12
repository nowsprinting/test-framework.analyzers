using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    public partial class PartialClass : NUnitAttribute
    {
        public ActionTargets Targets => ActionTargets.Test;
    }

    public partial class PartialClass : ITestAction // UTF5004
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
        }
    }
}
