using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class HasExactlyProperty
    {
        [Test]
        public void Test()
        {
            var files = new List<FileInfo> { new FileInfo("a.bin") };
            Assert.That(files, Has.Exactly(1).Property("Length").GreaterThan(0)); // UTF2006
        }
    }
}
