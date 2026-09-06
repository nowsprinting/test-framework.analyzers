using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CallToThrowingMethod : NUnitAttribute, IApplyToContext
    {
        public void ApplyToContext(ITestExecutionContext context)
        {
            context.TestCaseTimeout = ReadTimeout(); // UTF5001
        }

        private static int ReadTimeout()
        {
            throw new InvalidOperationException();
        }
    }
}
