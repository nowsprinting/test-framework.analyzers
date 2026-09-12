using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5002
{
    public partial class PartialClass : NUnitAttribute
    {
    }

    public partial class PartialClass : IWrapTestMethod // UTF5002
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
