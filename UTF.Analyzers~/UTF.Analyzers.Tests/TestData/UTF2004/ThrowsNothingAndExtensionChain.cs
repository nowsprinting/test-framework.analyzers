using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2004
{
    public class ThrowsNothingAndExtensionChain
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await Task.Yield(), Throws.Nothing.And.Not.AllocatingGCMemory());
            // Reported by UTF2002: the async delegate is owned by the Throws constraint rule.
        }
    }
}
