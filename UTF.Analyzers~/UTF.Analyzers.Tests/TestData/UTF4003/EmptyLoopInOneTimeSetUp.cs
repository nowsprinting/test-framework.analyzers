using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class EmptyLoopInOneTimeSetUp
    {
        private bool _flag;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            while (!_flag)   // UTF4003
            {
            }
        }

        [Test]
        public void Test()
        {
        }
    }
}
