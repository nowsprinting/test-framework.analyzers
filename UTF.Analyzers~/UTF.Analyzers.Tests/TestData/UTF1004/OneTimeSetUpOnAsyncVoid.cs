using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class OneTimeSetUpOnAsyncVoid
    {
        [OneTimeSetUp] // UTF1004
        public async void OneTimeSetUp()
        {
            await Task.Yield();
        }
    }
}
