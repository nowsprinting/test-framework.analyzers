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
    }}
