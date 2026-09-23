using System.Collections.Generic;

namespace UTF.Analyzers.Tests.TestData.UTF1008
{
    public class GenericEnumeratorWithoutUnityTest
    {
        public IEnumerator<object> Helper()
        {
            yield return null;
        }
    }
}
