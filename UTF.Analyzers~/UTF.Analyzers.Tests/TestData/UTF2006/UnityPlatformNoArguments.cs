using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class UnityPlatformNoArguments
    {
        [Test]
        [UnityPlatform]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128)); // UTF2006
        }
    }
}
