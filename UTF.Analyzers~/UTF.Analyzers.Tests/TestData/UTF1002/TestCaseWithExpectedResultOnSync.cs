using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1002
{
    public class TestCaseWithExpectedResultOnSync
    {
        [TestCase(1, ExpectedResult = 2)]
        public int Increment(int value)
        {
            return value + 1;
        }
    }
}
