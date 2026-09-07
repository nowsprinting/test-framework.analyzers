using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF3005
{
    public class DerivedFromTestAttribute : TestAttribute
    {
    }

    public class ImplementsSimpleTestBuilderAttribute : NUnitAttribute, ISimpleTestBuilder
    {
    }

    public class CustomTestAttributes
    {
        [DerivedFromTest]
        public void Add_TwoPositiveNumbers_ReturnsSum() // CA1707
        {
        }

        [ImplementsSimpleTestBuilder]
        public void Add_TwoNegativeNumbers_ReturnsSum() // CA1707
        {
        }
    }
}
