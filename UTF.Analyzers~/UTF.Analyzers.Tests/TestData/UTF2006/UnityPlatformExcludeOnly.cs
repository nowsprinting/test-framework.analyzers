using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class UnityPlatformExcludeOnly
    {
        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXPlayer, RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer })]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128)); // UTF2006
        }
    }
}
