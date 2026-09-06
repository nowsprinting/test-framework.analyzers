using System.Collections;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1003
{
    public class PairwiseOnEnumerable
    {
        [Pairwise]
        public IEnumerable MyTest([Values(1, 2)] int a, [Values(3, 4)] int b)
        {
            yield return a + b;
        }
    }
}
