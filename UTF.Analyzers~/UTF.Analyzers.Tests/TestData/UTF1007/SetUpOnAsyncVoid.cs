using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class SetUpOnAsyncVoid
    {
        [SetUp]
        public async void SetUp() // UTF1007
        {
            await Task.Yield();
        }
    }
}
