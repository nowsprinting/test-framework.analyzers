using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ActualValueDelegate
    {
        [Test]
        public void Test()
        {
            Assert.That(() => new FileInfo("a.bin"), Has.Length.GreaterThan(128)); // UTF2006
        }
    }
}
