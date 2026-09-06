using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class WrapperOnSync
    {
        [Test]
        [WrapperOnSyncWrapper]
        public void MySyncTest()
        {
        }
    }

    public class WrapperOnSyncWrapperAttribute : NUnitAttribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }}
