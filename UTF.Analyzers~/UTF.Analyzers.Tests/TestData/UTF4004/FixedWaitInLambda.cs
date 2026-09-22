using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class FixedWaitInLambda
    {
        [Test]
        public async Task Test()
        {
            Func<Task> settle = async () =>
            {
                await Task.Delay(1000);   // UTF4004
            };
            await settle();
        }
    }
}
