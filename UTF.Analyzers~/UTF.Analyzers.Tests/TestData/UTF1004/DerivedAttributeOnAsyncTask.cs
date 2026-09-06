using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1004
{
    public class DerivedAttributeOnAsyncTask
    {
        [DerivedOneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await Task.Yield();
        }
    }

    public class DerivedOneTimeSetUpAttribute : OneTimeSetUpAttribute
    {
    }
}
