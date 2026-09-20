using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class DoLoop
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            do   // UTF4003
            {
            } while (!_flag);
        }
    }
}
