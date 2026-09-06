using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class OneTimeSetUpOnNonAsyncValueTask
    {
        [OneTimeSetUp]
        public ValueTask OneTimeSetUp()
        {
            return default;
        }
    }
}
