using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ThrowsArgumentExceptionWithProperty
    {
        private static void Load(string path)
        {
            throw new ArgumentException("message", nameof(path));
        }

        [Test]
        public void Test()
        {
            Assert.That(() => Load(null!), Throws.ArgumentException.With.Property("ParamName")); // UTF2006
        }
    }
}
