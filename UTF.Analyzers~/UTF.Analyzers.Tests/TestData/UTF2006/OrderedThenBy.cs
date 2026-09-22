using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class OrderedThenBy
    {
        [Test]
        public void Test()
        {
            var files = new[] { new FileInfo("a.bin") };
            Assert.That(files, Is.Ordered.By("DirectoryName").Then.By("Length")); // UTF2006
        }
    }
}
