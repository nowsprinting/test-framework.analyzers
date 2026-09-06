using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CatchOfDerivedType : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            try
            {
                throw new Exception(); // UTF5001
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
