using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class RecursiveCallee : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            First(0); // UTF5001
        }

        private static void First(int depth)
        {
            Second(depth + 1);
        }

        private static void Second(int depth)
        {
            if (depth > 10)
            {
                throw new InvalidOperationException();
            }

            First(depth);
        }
    }
}
