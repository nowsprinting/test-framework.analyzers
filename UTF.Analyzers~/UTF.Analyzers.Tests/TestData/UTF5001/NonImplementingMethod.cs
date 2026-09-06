using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class NonImplementingMethod : NUnitAttribute
    {
        public void ApplyToTest(Test test)
        {
            throw new InvalidOperationException();
        }

        public void ApplyToContext(ITestExecutionContext context)
        {
            throw new InvalidOperationException();
        }

        public TestCommand Wrap(TestCommand command)
        {
            throw new InvalidOperationException();
        }
    }
}
