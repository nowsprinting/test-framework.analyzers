using System.Collections.Generic;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001;

public class TestCaseOnGenericEnumerator
{
    [TestCase(1)]
    public IEnumerator<int> MyTest(int value)
    {
        yield return value;
    }
}
