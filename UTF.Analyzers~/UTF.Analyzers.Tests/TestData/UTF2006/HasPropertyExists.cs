using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class HasPropertyExists
    {
        [Test]
        public void Test()
        {
            Assert.That(new ArgumentException("message", "path"), Has.Property("ParamName")); // UTF2006
        }
    }
}
