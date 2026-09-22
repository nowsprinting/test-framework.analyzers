using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class NonConstantName
    {
        [Test]
        public void Test()
        {
            var name = "Length";
            Assert.That(new FileInfo("a.bin"), Has.Property(name));
        }
    }
}
