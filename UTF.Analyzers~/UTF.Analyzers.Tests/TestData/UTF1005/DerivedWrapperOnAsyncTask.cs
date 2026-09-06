using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class DerivedWrapperOnAsyncTask
    {
        [Test]
        [DerivedWrapperOnAsyncTaskDerived] // UTF1005
        public async Task MyAsyncTest()
        {
            await Task.Yield();
        }
    }

    public class DerivedWrapperOnAsyncTaskBaseAttribute : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
    public class DerivedWrapperOnAsyncTaskDerivedAttribute : DerivedWrapperOnAsyncTaskBaseAttribute
    {
    }
}
