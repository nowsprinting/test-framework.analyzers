using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class DeadlineLoopInHelper
    {
        [Test]
        public async Task Test()
        {
            await RotateCube();
        }

        private static async Task RotateCube()
        {
            var startTime = Time.time;
            while (Time.time - startTime < 1.0f)
            {
                await Task.Yield();
            }
        }
    }
}
