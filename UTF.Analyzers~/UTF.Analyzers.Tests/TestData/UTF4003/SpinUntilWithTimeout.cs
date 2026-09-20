using System;
using System.Threading;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class SpinUntilWithTimeout
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            var a = SpinWait.SpinUntil(() => _flag, 1000);   // UTF4003
            var b = SpinWait.SpinUntil(() => _flag, TimeSpan.FromSeconds(1));   // UTF4003
            _flag = a && b;
        }
    }
}
