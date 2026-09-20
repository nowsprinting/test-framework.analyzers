using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class EmptyLoopInAsyncVoidTearDown
    {
        private bool _flag;

        [TearDown]
        public async void TearDown()
        {
            while (!_flag)   // UTF4003
            {
            }

            await Task.Yield();
        }

        [Test]
        public void Test()
        {
        }
    }
}
