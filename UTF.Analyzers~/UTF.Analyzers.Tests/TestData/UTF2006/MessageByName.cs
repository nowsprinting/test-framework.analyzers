using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class MessageByName
    {
        [Test]
        public void Test()
        {
            Assert.That(new InvalidOperationException("boom"), Has.Property("Message").EqualTo("boom")); // UTF2006
        }
    }
}
