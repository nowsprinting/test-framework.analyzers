using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF5003
{
    public interface IDerivedInterfaceAction : IOuterUnityTestAction
    {
    }

    public class DerivedInterface : NUnitAttribute, IDerivedInterfaceAction // UTF5003
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
