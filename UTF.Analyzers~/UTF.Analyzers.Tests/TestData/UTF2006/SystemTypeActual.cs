using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class SystemTypeActual
    {
        [Test]
        public void Test()
        {
            Assert.That(typeof(FileInfo), Has.Property("IsInterface"));
        }
    }
}
