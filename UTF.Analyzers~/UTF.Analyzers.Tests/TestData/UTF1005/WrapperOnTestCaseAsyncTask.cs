using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class WrapperOnTestCaseAsyncTask
    {
        [TestCase(1)]
        [WrapperOnTestCaseAsyncTaskWrapper] // UTF1005
        public async Task MyAsyncTest(int value)
        {
            await Task.Yield();
        }
    }

    public class WrapperOnTestCaseAsyncTaskWrapperAttribute : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
