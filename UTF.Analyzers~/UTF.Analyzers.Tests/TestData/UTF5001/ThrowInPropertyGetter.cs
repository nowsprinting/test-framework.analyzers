using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class ThrowInPropertyGetter : NUnitAttribute, IApplyToContext
    {
        private static int Timeout => throw new InvalidOperationException();

        public void ApplyToContext(ITestExecutionContext context)
        {
            context.TestCaseTimeout = Timeout;
        }
    }
}
