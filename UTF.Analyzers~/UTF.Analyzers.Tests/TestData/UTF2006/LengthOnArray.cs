using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class LengthOnArray
    {
        [Test]
        public void Test()
        {
            Assert.That(new[] { 1, 2, 3 }, Has.Length.EqualTo(3));
        }
    }
}
