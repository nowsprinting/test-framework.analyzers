using System.Collections;

namespace UTF.Analyzers.Tests.TestData.UTF1009
{
    public class EnumerableWithoutAttribute
    {
        public IEnumerable SetUp()
        {
            yield return null;
        }
    }
}
