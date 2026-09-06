using System.Collections.Generic;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1003
{
    public class PairwiseOnGenericEnumerator
    {
        [Pairwise]
        public IEnumerator<int> MyTest([Values(1, 2)] int a, [Values(3, 4)] int b)
        {
            yield return a + b;
        }
    }
}
