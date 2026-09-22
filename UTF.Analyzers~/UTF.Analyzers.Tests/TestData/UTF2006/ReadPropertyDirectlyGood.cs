using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ReadPropertyDirectlyGood
    {
        [Test]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin").Length, Is.GreaterThan(128));
            var ex = new ArgumentNullException("path");
            Assert.That(ex.ParamName, Is.EqualTo("path"));
        }
    }
}
