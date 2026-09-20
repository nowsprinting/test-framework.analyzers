using System;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInLambda
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            Action wait = () =>
            {
                while (!_flag)   // UTF4003
                {
                }
            };
            wait();
        }
    }
}
