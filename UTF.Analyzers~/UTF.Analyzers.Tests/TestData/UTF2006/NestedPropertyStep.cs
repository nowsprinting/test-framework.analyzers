using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class NestedPropertyStep
    {
        private class FileHolder
        {
            public FileInfo File { get; } = new FileInfo("a.bin");
        }

        [Test]
        public void Test()
        {
            Assert.That(new FileHolder(), Has.Property("File").With.Length.GreaterThan(0)); // UTF2006
        }
    }
}
