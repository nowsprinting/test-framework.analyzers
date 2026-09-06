using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class OverrideInIntermediateBase : OverrideInIntermediateBaseMiddle
    {
    }

    public class OverrideInIntermediateBaseMiddle : OverrideInIntermediateBaseRoot
    {
        public override void ApplyToTest(Test test)
        {
            throw new InvalidOperationException(); // UTF5001
        }
    }

    public class OverrideInIntermediateBaseRoot : NUnitAttribute, IApplyToTest
    {
        public virtual void ApplyToTest(Test test)
        {
        }
    }
}
