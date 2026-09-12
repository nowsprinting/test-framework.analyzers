using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF5003
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public partial class PartialWidensBase : PartialWidensBaseAttribute // UTF5003
    {
    }

    public partial class PartialWidensBase
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class PartialWidensBaseAttribute : NUnitAttribute, IOuterUnityTestAction
    {
        public IEnumerator BeforeTest(ITest test)
        {
            yield return null;
        }

        public IEnumerator AfterTest(ITest test)
        {
            yield return null;
        }
    }
}
