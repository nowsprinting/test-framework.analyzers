using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class ExplicitImplementation : NUnitAttribute, IApplyToTest
    {
        void IApplyToTest.ApplyToTest(Test test)
        {
            throw new InvalidOperationException(); // UTF5001
        }
    }
}
