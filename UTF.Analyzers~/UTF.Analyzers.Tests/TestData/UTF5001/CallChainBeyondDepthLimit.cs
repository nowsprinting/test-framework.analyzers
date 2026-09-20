using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UTF.Analyzers.Tests.TestData.UTF5001
{
    public class CallChainBeyondDepthLimit : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            Depth1();
        }

        private static void Depth1()
        {
            Depth2();
        }

        private static void Depth2()
        {
            Depth3();
        }

        private static void Depth3()
        {
            Depth4();
        }

        private static void Depth4()
        {
            Depth5();
        }

        private static void Depth5()
        {
            Depth6();
        }

        private static void Depth6()
        {
            throw new InvalidOperationException();
        }
    }
}
