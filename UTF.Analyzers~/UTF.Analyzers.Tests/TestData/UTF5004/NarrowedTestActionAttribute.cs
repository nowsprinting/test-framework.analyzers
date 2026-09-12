using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF5004
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
    public class NarrowedTestActionAttribute : TestActionAttribute
    {
        public override void BeforeTest(ITest test)
        {
        }
    }
}
