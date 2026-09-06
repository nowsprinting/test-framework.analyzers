using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class MultipleSites : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test)); // UTF5001
            }

            Validate(); // UTF5001
        }

        private static void Validate()
        {
            throw new InvalidOperationException();
        }
    }
}
