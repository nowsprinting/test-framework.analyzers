using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class CountOnHashSet
    {
        [Test]
        public void Test()
        {
            Assert.That(new HashSet<int> { 1 }, Has.Count.EqualTo(1));
        }
    }
}
