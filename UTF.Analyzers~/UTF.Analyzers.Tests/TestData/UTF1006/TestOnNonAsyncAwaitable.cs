using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class TestOnNonAsyncAwaitable
    {
        [Test]
        public Awaitable ReturnsAwaitable() // UTF1006
        {
            return new Awaitable();
        }
    }
}
