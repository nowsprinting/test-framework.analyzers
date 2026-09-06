using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class PartiallyCaughtCallee : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            try
            {
                Validate(test); // UTF5001
            }
            catch (ArgumentException)
            {
            }
        }

        private static void Validate(Test test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            throw new InvalidOperationException();
        }
    }
}
