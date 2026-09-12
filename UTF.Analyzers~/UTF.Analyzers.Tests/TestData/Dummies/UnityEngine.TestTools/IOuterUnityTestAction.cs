// Dummy of UnityEngine.TestTools.IOuterUnityTestAction. Declaration-only; see test-data-conventions.md.

using System.Collections;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools
{
    public interface IOuterUnityTestAction
    {
        IEnumerator BeforeTest(ITest test);

        IEnumerator AfterTest(ITest test);
    }
}
