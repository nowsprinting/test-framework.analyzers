using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ListMapProperty
    {
        [Test]
        public void Test()
        {
            var files = new List<FileInfo> { new FileInfo("a.bin") };
            Assert.That(List.Map(files).Property("Length"), Is.EqualTo(new[] { 0L })); // UTF2006
        }
    }
}
