using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class OneTimeSetUpOnAsyncVoid
    {
        [OneTimeSetUp]
        public async void OneTimeSetUp()
        {
            await Task.Yield();
        }
    }
}
