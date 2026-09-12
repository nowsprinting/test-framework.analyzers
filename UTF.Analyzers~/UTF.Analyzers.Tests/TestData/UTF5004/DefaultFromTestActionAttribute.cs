using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    public class DefaultFromTestActionAttribute : TestActionAttribute // UTF5004
    {
        public override void BeforeTest(ITest test)
        {
        }
    }
}
