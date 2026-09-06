using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class Rethrow : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            try
            {
                test.RunState = RunState.Runnable;
            }
            catch
            {
                throw; // UTF5001
            }
        }
    }
}
