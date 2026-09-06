using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CallToThrowingLocalFunction : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            Validate(); // UTF5001

            void Validate()
            {
                throw new InvalidOperationException();
            }
        }
    }
}
