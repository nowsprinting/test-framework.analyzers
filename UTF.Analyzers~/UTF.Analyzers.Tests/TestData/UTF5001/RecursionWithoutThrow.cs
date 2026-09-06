using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class RecursionWithoutThrow : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            test.RunState = Count(3) > 0 ? RunState.Runnable : RunState.NotRunnable;
        }

        private static int Count(int depth)
        {
            return depth <= 0 ? 0 : 1 + Count(depth - 1);
        }
    }
}
