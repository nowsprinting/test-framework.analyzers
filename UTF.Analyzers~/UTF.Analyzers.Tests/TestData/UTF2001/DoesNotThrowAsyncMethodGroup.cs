using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class DoesNotThrowAsyncMethodGroup
    {
        [Test]
        public void DoesNotThrowAsyncFreezes()
        {
            Assert.DoesNotThrowAsync(BarAsync); // UTF2001
        }

        private static async Task BarAsync()
        {
            await Task.Yield();
        }
    }
}
