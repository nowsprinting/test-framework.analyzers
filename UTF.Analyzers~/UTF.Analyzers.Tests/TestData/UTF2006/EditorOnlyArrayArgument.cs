using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyArrayArgument
    {
        [Test]
        [UnityPlatform(new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor })]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
        }
    }
}
