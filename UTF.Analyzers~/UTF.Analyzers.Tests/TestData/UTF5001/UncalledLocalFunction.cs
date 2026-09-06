using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class UncalledLocalFunction : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            test.RunState = RunState.Runnable;

            void Fail()
            {
                throw new InvalidOperationException();
            }
        }
    }
}
