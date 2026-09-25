using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyPropertyInVariable
    {
        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor)]
        public void Test()
        {
            var constraint = Has.Property("Length").GreaterThan(128);
            Assert.That(new FileInfo("a.bin"), constraint);
        }
    }
}
