using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class EditorOnlyInLambda
    {
        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor)]
        public void Test()
        {
            Action check = () => Assert.That(new FileInfo("a.bin"), Has.Length.GreaterThan(128));
            check();
        }
    }
}
