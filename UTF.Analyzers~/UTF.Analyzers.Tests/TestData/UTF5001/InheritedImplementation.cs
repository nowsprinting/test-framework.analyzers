using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class InheritedImplementation : InheritedImplementationBase, IApplyToTest
    {
    }

    public class InheritedImplementationBase : NUnitAttribute
    {
        public void ApplyToTest(Test test)
        {
            throw new InvalidOperationException(); // UTF5001
        }
    }
}
