using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5002
{
    public class NonAttributeClass : IWrapTestMethod // UTF5002
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
