using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class GoodExample : NUnitAttribute, IApplyToTest, IApplyToContext, IWrapTestMethod
    {
        private const int DefaultTimeout = 30000;
        private readonly string _key;

        public GoodExample(string key)
        {
            _key = key;
        }

        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable || test.RunState == RunState.Ignored)
            {
                return;
            }

            if (Environment.GetEnvironmentVariable(_key) == null)
            {
                test.RunState = RunState.NotRunnable;
            }
        }

        public void ApplyToContext(ITestExecutionContext context)
        {
            context.TestCaseTimeout = int.TryParse(Environment.GetEnvironmentVariable(_key + ".timeout"), out var timeout)
                ? timeout
                : DefaultTimeout;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new GoodExampleCommand(command);
        }
    }

    public class GoodExampleCommand : TestCommand
    {
        private readonly TestCommand _innerCommand;

        public GoodExampleCommand(TestCommand innerCommand)
        {
            _innerCommand = innerCommand;
        }

        public TestCommand InnerCommand => _innerCommand;
    }
}
