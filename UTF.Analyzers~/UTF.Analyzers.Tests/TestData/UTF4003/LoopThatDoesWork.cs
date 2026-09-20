using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopThatDoesWork
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            var count = 0;
            while (!_flag)
            {
                count++;
            }

            while (true)
            {
                if (_flag)
                {
                    break;
                }
            }

            _flag = count > 0;
        }
    }
}
