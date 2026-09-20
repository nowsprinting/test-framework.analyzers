using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class EmptyStatementLoop
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            while (!_flag) ;   // UTF4003
        }
    }
}
