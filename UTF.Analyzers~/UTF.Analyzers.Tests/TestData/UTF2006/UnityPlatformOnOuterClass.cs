using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    [UnityPlatform(RuntimePlatform.OSXEditor)]
    public class UnityPlatformOnOuterClass
    {
        public class NestedFixture
        {
            [Test]
            public void Test()
            {
                Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128)); // UTF2006
            }
        }
    }
}
