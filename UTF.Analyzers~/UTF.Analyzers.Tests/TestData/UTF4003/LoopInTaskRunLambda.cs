using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4003
{

    public class LoopInTaskRunLambda
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            Task.Run(() =>
            {
                while (!_flag)   // UTF4003
                {
                }
            }).Wait();
        }
    }
}
