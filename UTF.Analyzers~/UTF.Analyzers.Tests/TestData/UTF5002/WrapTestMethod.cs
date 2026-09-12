using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5002
{
    public class WrapTestMethod : NUnitAttribute, IWrapTestMethod // UTF5002
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
