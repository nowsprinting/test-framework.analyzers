using System;
using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsNothingAwaitableMethodGroup
    {
        [Test]
        public void Test()
        {
            Assert.That(FooAsync, Throws.Nothing); // UTF2002
        }

        private static Awaitable FooAsync()
        {
            throw new InvalidOperationException("boom");
        }
    }
}
