using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class ThrowInFinally : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            try
            {
                test.RunState = RunState.Runnable;
            }
            finally
            {
                throw new InvalidOperationException(); // UTF5001
            }
        }
    }
}
