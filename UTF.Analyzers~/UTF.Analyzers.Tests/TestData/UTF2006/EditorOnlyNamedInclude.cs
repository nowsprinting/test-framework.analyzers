using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyNamedInclude
    {
        [Test]
        [UnityPlatform(include = new[] { RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor })]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
        }
    }
}
