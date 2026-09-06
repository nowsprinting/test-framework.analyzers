using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CalleeCatchesInternally : NUnitAttribute, IApplyToContext
    {
        public void ApplyToContext(ITestExecutionContext context)
        {
            context.TestCaseTimeout = ReadTimeout();
        }

        private static int ReadTimeout()
        {
            try
            {
                return int.Parse(Environment.GetEnvironmentVariable("TIMEOUT"));
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
