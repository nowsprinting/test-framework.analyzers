using System.Collections;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF4007
{
    public abstract class OverriddenBaseDeclarationBase
    {
        [UnitySetUp]
        public abstract IEnumerator SetUp();
    }

    public class OverriddenBaseDeclaration : OverriddenBaseDeclarationBase
    {
        public override IEnumerator SetUp() // UTF4007
        {
            yield return null;
        }
    }
}
