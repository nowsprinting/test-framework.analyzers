using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class PropertyNotFoundOnStaticType
    {
        private static void Load()
        {
            throw new ArgumentNullException("path");
        }

        [Test]
        public void Test()
        {
            FileSystemInfo info = new FileInfo("a.bin");
            Assert.That(info, Has.Length.GreaterThan(128));
            Assert.That(() => Load(), Throws.Exception.With.Property("ParamName"));
        }
    }
}
