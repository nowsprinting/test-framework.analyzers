using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyAmongMultiple
    {
        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.Android })]
        [UnityPlatform(RuntimePlatform.LinuxEditor)]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
        }
    }
}
