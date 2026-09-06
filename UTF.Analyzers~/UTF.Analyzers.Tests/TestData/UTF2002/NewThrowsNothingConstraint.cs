using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class NewThrowsNothingConstraint
    {
        [Test]
        public void Test()
        {
            Assert.That(BarAsync, new ThrowsNothingConstraint()); // UTF2002
        }

        private static async Task BarAsync()
        {
            await Task.Yield();
        }
    }
}
