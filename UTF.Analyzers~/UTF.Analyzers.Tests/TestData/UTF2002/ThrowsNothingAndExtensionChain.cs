using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsNothingAndExtensionChain
    {
        [Test]
        public void Test()
        {
            Assert.That(async () => await Task.Yield(), Throws.Nothing.And.Not.AllocatingGCMemory()); // UTF2002
        }
    }
}
