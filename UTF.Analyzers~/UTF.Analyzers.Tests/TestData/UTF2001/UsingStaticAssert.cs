using System.Threading.Tasks;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class UsingStaticAssert
    {
        [Test]
        public void DoesNotThrowAsyncFreezes()
        {
            DoesNotThrowAsync(BarAsync); // UTF2001
        }

        private static async Task BarAsync()
        {
            await Task.Yield();
        }
    }
}
