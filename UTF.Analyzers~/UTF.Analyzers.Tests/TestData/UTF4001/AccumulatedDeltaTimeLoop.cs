using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class AccumulatedDeltaTimeLoop
    {
        private bool _flag;
        private float _elapsed;

        [Test]
        public async Task LocalSum()
        {
            var elapsed = 0f;
            while (elapsed < 1.0f)
            {
                elapsed += Time.deltaTime;
                await Task.Yield();
            }
        }

        [Test]
        public async Task FieldSumWithFlag()
        {
            _elapsed = 0f;
            while (!_flag && _elapsed < 1.0f)
            {
                await Task.Yield();
                _elapsed += Time.deltaTime;
            }
        }

        [Test]
        public async Task AssignedFromClock()
        {
            var startTime = Time.time;
            var elapsed = 0f;
            do
            {
                await Task.Yield();
                elapsed = Time.time - startTime;
            } while (elapsed < 1.0f);
        }
    }
}
