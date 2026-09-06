using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class WrapperOnSetUp
    {
        [SetUp]
        [WrapperOnSetUpWrapper]
        // A wrapper attribute on a non-test method is not diagnosed: Unity Test Framework reads wrapper attributes
        // only from the test method, so the attribute here does nothing at all.
        public async Task MySetUp()
        {
            await Task.Yield();
        }
    }

    public class WrapperOnSetUpWrapperAttribute : NUnitAttribute, IWrapSetUpTearDown
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
