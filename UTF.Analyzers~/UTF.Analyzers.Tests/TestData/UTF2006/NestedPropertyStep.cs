using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class NestedPropertyStep
    {
        [Test]
        public void Test()
        {
            Assert.That(new FileInfo("a.bin"), Has.Property("Directory").With.Property("Parent").Not.EqualTo(null!)); // UTF2006
        }
    }
}
