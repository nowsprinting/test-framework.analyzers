using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class MultipleWrappersOnAsyncTask
    {
        [Test]
        [MultipleWrappersOnAsyncTaskFirst] // UTF1005
        [MultipleWrappersOnAsyncTaskSecond] // UTF1005
        public async Task MyAsyncTest()
        {
            await Task.Yield();
        }
    }

    public class MultipleWrappersOnAsyncTaskFirstAttribute : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }
    public class MultipleWrappersOnAsyncTaskSecondAttribute : NUnitAttribute, IWrapSetUpTearDown
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }}
