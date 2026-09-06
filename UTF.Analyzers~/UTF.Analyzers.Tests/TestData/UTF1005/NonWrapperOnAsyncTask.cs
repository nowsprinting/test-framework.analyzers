using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class NonWrapperOnAsyncTask
    {
        [Test]
        [NonWrapperOnAsyncTaskMarker]
        public async Task MyAsyncTest()
        {
            await Task.Yield();
        }
    }

    public class NonWrapperOnAsyncTaskMarkerAttribute : NUnitAttribute
    {
    }
}
