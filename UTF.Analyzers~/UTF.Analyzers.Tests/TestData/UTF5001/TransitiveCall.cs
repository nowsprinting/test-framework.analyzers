using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class TransitiveCall : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            Outer(); // UTF5001
        }

        private static void Outer()
        {
            Inner();
        }

        private static void Inner()
        {
            throw new InvalidOperationException();
        }
    }
}
