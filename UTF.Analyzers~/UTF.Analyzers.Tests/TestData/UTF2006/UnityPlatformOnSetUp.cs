using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class UnityPlatformOnSetUp
    {
        [SetUp]
        [UnityPlatform(RuntimePlatform.OSXEditor)]
        public void SetUp()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(0)); // UTF2006
        }

        [Test]
        public void Test()
        {
        }
    }
}
