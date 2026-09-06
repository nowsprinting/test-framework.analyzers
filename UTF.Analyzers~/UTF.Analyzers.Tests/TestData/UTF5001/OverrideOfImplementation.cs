using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class OverrideOfImplementation : OverrideOfImplementationBase
    {
        public override void ApplyToTest(Test test)
        {
            throw new InvalidOperationException(); // UTF5001
        }
    }

    public class OverrideOfImplementationBase : NUnitAttribute, IApplyToTest
    {
        public virtual void ApplyToTest(Test test)
        {
        }
    }
}
