using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class ExternalMethod : NUnitAttribute, IApplyToContext
    {
        public void ApplyToContext(ITestExecutionContext context)
        {
            context.TestCaseTimeout = int.Parse(Environment.GetEnvironmentVariable("TIMEOUT") ?? "");
        }
    }
}
