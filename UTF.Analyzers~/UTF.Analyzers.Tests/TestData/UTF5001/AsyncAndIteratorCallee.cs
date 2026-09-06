using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class AsyncAndIteratorCallee : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            var task = FailAsync();
            var values = Iterate();
            test.RunState = task.IsCompleted && values != null ? RunState.Runnable : RunState.NotRunnable;
        }

        private static async Task FailAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException();
        }

        private static IEnumerable<int> Iterate()
        {
            yield return 1;
            throw new InvalidOperationException();
        }
    }
}
