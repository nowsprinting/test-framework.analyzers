using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class NameofPropertyName
    {
        [Test]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Property(nameof(FileInfo.Length)).GreaterThan(128)); // UTF2006
        }
    }
}
