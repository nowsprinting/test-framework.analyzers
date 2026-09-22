using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class ExceptionMessageOverride
    {
        private static void Load(string path)
        {
            throw new ArgumentNullException(nameof(path));
        }

        [Test]
        public void Test()
        {
            Assert.That(() => Load(null!), Throws.ArgumentNullException.With.Message.Contains("path"));
        }
    }
}
