using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class PropertyInVariable
    {
        [Test]
        public void Test()
        {
            var constraint = Has.Property("Length").GreaterThan(128); // UTF2006
            Assert.That(new FileInfo("a.bin"), constraint);
        }
    }
}
