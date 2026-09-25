using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[assembly: UnityPlatform(RuntimePlatform.OSXEditor)]

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyAssembly
    {
        [Test]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
        }
    }
}
