using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ConstraintInVariable
    {
        [Test]
        public void Test()
        {
            var constraint = Has.Length.GreaterThan(128);
            Assert.That(new FileInfo("a.bin"), constraint);
        }
    }
}
