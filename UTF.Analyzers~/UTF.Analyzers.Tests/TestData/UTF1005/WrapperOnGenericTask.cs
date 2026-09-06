using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class WrapperOnGenericTask
    {
        [Test]
        [WrapperOnGenericTaskWrapper]
        public async Task<int> MyAsyncTest()
        {
            await Task.Yield();
            return 1;
        }
    }

    public class WrapperOnGenericTaskWrapperAttribute : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
}
