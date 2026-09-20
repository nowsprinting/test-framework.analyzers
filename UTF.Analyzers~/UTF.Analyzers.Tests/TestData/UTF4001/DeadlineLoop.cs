using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class DeadlineLoop
    {
        private bool _flag;

        [Test]
        public async Task ScaledTime()
        {
            var startTime = Time.time;
            while (Time.time - startTime < 1.0f)
            {
                await Task.Yield();
            }
        }

        [Test]
        public async Task FrameCount()
        {
            var deadline = Time.frameCount + 10;
            while (!_flag && Time.frameCount < deadline)
            {
                await Task.Yield();
            }
        }

        [Test]
        public async Task WallClock()
        {
            var deadline = DateTime.UtcNow.AddSeconds(1);
            do
            {
                await Task.Yield();
            } while (DateTime.UtcNow < deadline);
        }

        [Test]
        public async Task StopwatchElapsed()
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 1000)
            {
                await Task.Yield();
            }
        }
    }
}
