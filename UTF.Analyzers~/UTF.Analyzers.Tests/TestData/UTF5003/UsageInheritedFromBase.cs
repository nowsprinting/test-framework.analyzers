using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF5003
{
    public class UsageInheritedFromBase : UsageInheritedFromBaseAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class UsageInheritedFromBaseAttribute : NUnitAttribute, IOuterUnityTestAction
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
