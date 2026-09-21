using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class TaskDelayInAsync
    {
        [Test]
        public async Task Test()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));   // UTF4004
        }
    }
}
