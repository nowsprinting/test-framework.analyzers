using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001;

public class TestCaseOnVoid
{
    [TestCase(1)]
    [TestCase(2)]
    public void MySyncTest(int value)
    {
    }
}
