using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class HelperBeyondDepthLimit
    {
        private bool _flag;

        [Test]
        public async Task Test()
        {
            await Depth1();
        }

        private async Task Depth1()
        {
            await Depth2();
        }

        private async Task Depth2()
        {
            await Depth3();
        }

        private async Task Depth3()
        {
            while (!_flag)
            {
            }

            await Task.Yield();
        }
    }
}
