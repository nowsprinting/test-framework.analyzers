using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInNonTestMethod
    {
        private bool _flag;

        public void NotATest()
        {
            while (!_flag)
            {
            }
        }

        [Test]
        public void Test()
        {
        }
    }
}
