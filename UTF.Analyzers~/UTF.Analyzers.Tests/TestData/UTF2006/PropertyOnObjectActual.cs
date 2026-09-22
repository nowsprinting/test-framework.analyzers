using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class PropertyOnObjectActual
    {
        [Test]
        public void Test()
        {
            object actual = new FileInfo("a.bin");
            Assert.That(actual, Has.Property("Length").GreaterThan(128)); // UTF2006
        }
    }
}
