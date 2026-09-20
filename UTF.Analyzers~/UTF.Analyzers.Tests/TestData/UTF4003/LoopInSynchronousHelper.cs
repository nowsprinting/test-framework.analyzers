using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInSynchronousHelper
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            WaitForFlag();
        }

        private void WaitForFlag()
        {
            while (!_flag)
            {
            }
        }
    }
}
