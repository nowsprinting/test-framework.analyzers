using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInLocalFunction
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            Wait();

            void Wait()
            {
                while (!_flag)   // UTF4003
                {
                }
            }
        }
    }
}
