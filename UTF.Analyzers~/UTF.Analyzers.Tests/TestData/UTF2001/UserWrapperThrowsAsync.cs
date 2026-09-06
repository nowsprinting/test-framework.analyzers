using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class UserWrapperThrowsAsync
    {
        [Test]
        public void WrapperIsNotDetected()
        {
            MyAssert.ThrowsAsync<InvalidOperationException>(FooAsync);
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }

    public static class MyAssert
    {
        public static TActual ThrowsAsync<TActual>(AsyncTestDelegate code) where TActual : Exception
        {
            throw new NotImplementedException();
        }
    }
}
