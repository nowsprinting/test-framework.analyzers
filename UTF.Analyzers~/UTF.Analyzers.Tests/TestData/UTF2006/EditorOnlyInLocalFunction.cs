using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyInLocalFunction
    {
        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor)]
        public void Test()
        {
            Check();

            void Check() => Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
        }
    }
}
