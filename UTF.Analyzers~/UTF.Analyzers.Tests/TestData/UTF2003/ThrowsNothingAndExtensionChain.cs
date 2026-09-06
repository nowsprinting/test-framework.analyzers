using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class ThrowsNothingAndExtensionChain
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await Task.Yield(), Throws.Nothing.And.Not.AllocatingGCMemory());
            // Reported by UTF2002: the constraint chain is rooted in Throws.
        }
    }
}
