using System;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class SyncThrows
    {
        [Test]
        public void ThrowsIsSafe()
        {
            Assert.Throws<InvalidOperationException>(() => throw new InvalidOperationException("boom"));
        }
    }
}
