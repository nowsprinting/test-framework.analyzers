using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class HasSomePropertyOnArray
    {
        [Test]
        public void Test()
        {
            var files = new[] { new FileInfo("a.bin") };
            Assert.That(files, Has.Some.Property("Length").GreaterThan(0)); // UTF2006
        }
    }
}
