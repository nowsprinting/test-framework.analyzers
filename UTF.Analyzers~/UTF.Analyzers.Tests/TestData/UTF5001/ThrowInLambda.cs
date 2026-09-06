using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class ThrowInLambda : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            Func<int> read = () => throw new InvalidOperationException();
            Action fail = delegate { throw new InvalidOperationException(); };
            test.RunState = read() > 0 ? RunState.Runnable : RunState.NotRunnable;
            fail();
        }
    }
}
