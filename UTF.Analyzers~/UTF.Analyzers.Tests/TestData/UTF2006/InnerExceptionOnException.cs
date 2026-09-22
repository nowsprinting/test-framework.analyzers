using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class InnerExceptionOnException
    {
        private static void Load()
        {
            throw new Exception("message", new InvalidOperationException());
        }

        [Test]
        public void Test()
        {
            Assert.That(() => Load(), Throws.Exception.With.InnerException.TypeOf<InvalidOperationException>());
        }
    }
}
