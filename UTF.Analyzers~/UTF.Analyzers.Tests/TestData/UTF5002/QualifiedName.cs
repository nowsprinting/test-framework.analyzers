using NUnit.Framework;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5002
{
    public class QualifiedName : NUnitAttribute, NUnit.Framework.Interfaces.IWrapTestMethod // UTF5002
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
