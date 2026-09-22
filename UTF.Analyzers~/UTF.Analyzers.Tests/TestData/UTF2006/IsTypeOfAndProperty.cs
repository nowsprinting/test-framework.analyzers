using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class IsTypeOfAndProperty
    {
        [Test]
        public void Test()
        {
            object actual = new FileInfo("a.bin");
            Assert.That(actual, Is.TypeOf<FileInfo>().And.Property("Length").GreaterThan(0)); // UTF2006
        }
    }
}
