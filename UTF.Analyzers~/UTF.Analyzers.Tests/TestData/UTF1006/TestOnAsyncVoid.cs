using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestOnAsyncVoid
    {
        [Test]
        public async void AwaitsYield()
        {
            await Task.Yield();
        }
    }
}
