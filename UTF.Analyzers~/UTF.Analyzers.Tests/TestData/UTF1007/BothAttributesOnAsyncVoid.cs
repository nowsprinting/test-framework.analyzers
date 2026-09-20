using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class BothAttributesOnAsyncVoid
    {
        [SetUp]
        [TearDown]
        public async void Reset() // UTF1007
        {
            await Task.Yield();
        }
    }
}
