using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestCaseWithExpectedResultOnGenericCollection
    {
        [TestCase(3, ExpectedResult = new[] { 0, 1, 2 })]
        public IEnumerable<int> Range(int count)
        {
            return Enumerable.Range(0, count);
        }
    }
}
