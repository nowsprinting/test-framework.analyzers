using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor)]
    public class EditorOnlyFixture
    {
        public static readonly IResolveConstraint LengthExists = Has.Property("Length");

        [SetUp]
        public void SetUp()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(0));
        }

        [Test]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
            AssertLength(new FileInfo("b.bin"));
        }

        private static void AssertLength(FileInfo file)
        {
            Assert.That(file, Has.Length.GreaterThan(128));
        }
    }
}
