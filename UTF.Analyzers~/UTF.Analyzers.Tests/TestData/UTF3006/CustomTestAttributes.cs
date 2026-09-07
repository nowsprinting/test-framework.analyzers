using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UTF.Analyzers.Tests.TestData.UTF3006
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
        public async Task AddTwoPositiveNumbers() // VSTHRD200
        {
            await Task.Yield();
        }

        [ImplementsSimpleTestBuilder]
        public async Task AddTwoNegativeNumbers() // VSTHRD200
        {
            await Task.Yield();
        }
    }
}
