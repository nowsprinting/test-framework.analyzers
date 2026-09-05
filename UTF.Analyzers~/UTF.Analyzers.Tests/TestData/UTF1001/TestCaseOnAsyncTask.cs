using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1001;

public class TestCaseOnAsyncTask
{
    [TestCase(1)]
    [TestCase(2)]
    public async Task MyAsyncTest(int value)
    {
        await Task.Yield();
    }
}
