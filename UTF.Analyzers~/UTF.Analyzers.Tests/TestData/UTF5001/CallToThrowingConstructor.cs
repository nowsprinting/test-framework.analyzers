using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CallToThrowingConstructor : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return new CallToThrowingConstructorCommand(command); // UTF5001
        }
    }

    public class CallToThrowingConstructorCommand : TestCommand
    {
        public CallToThrowingConstructorCommand(TestCommand innerCommand)
        {
            throw new ArgumentNullException(nameof(innerCommand));
        }
    }
}
