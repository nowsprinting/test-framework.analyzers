using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class TearDownOnNonAsyncAwaitable
    {
        [TearDown]
        public Awaitable TearDown() // UTF1007
        {
            return new Awaitable();
        }
    }
}
