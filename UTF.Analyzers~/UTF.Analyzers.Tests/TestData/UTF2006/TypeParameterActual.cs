using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class TypeParameterActual
    {
        [Test]
        public void Test<T>(T actual)
        {
            Assert.That(actual, Has.Property("Length"));
        }
    }
}
