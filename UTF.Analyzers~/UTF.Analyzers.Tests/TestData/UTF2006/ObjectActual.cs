using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ObjectActual
    {
        [Test]
        public void Test()
        {
            object actual = new FileInfo("a.bin");
            Assert.That(actual, Has.Length.GreaterThan(128));
        }
    }
}
