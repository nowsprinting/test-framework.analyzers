using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class LengthOnMemoryStream
    {
        [Test]
        public void Test()
        {
            Assert.That(new MemoryStream(), Has.Length.EqualTo(0));
        }
    }
}
