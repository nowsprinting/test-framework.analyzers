using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF5003
{
    public class NoAttributeUsage : NUnitAttribute, IOuterUnityTestAction // UTF5003
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
