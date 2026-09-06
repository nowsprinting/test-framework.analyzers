using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class WrapSetUpTearDownOnCoroutine
    {
        [UnityTest]
        [WrapSetUpTearDownOnCoroutineWrapper] // UTF1005
        public IEnumerator MyCoroutineTest()
        {
            yield return null;
        }
    }

    public class WrapSetUpTearDownOnCoroutineWrapperAttribute : NUnitAttribute, IWrapSetUpTearDown
    {
        public TestCommand Wrap(TestCommand command)
        {
            return command;
        }
    }}
