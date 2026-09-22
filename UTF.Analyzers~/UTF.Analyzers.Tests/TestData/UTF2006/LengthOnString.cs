using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class LengthOnString
    {
        [Test]
        public void Test()
        {
            Assert.That("abc", Has.Length.EqualTo(3));
        }
    }
}
