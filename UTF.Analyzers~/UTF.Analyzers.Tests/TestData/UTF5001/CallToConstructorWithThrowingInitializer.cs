using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CallToConstructorWithThrowingInitializer : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return new CallToConstructorWithThrowingInitializerDerived(command); // UTF5001
        }
    }

    public class CallToConstructorWithThrowingInitializerBase : TestCommand
    {
        public CallToConstructorWithThrowingInitializerBase(TestCommand innerCommand)
        {
            throw new ArgumentNullException(nameof(innerCommand));
        }
    }

    public class CallToConstructorWithThrowingInitializerDerived : CallToConstructorWithThrowingInitializerBase
    {
        public CallToConstructorWithThrowingInitializerDerived(TestCommand innerCommand) : base(innerCommand)
        {
        }
    }
}
